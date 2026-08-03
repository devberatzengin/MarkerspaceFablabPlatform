using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Equipment;
using Response = MakerspaceFablabPlatform.Dtos.Equipment.Response;
using EquipmentRentalResponse = MakerspaceFablabPlatform.Dtos.EquipmentRental.Response;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using MakerspaceFablabPlatform.States.EquipmentStates;
using MakerspaceFablabPlatform.Strategies.MembershipStrategies;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class EquipmentService : IEquipmentService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EquipmentService> _logger;
    private readonly IMapper _mapper;

    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    
    private readonly IStateFactory _stateFactory;

    private readonly IMembershipStrategy _membershipStrategy;

    public EquipmentService(IStateFactory stateFactory,IMembershipStrategy strategy, IMapper mapper, IUnitOfWork unitOfWork, ILogger<EquipmentService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _stateFactory = stateFactory;
        _membershipStrategy = strategy;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    
    public async Task<Response> GetByIdAsync(Guid id, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
     
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);

        return _mapper.Map<Response>(result);

    }

    public async Task<PagedResponse<Response>> GetAllAsync(ListRequest request, CancellationToken token)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Equipments.Query().Include("EquipmentRentals");

        if (request.Status is not null)
            query = query.Where(e => e.Status == request.Status);

        if (request.Type is not null)
            query = query.Where(e => e.Type == request.Type);

        if (request.PlacementType is not null)
            query = query.Where(e => e.PlacementType == request.PlacementType);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, pattern) ||
                EF.Functions.ILike(e.Description, pattern));
        }

        var totalCount = await query.CountAsync(token);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<Response>(_mapper.ConfigurationProvider)
            .ToListAsync(token);

        _logger.LogInformation("Listed {Count}/{Total} equipments (page {Page})", items.Count, totalCount, page);

        return new PagedResponse<Response>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Response> CreateAsync(CreateRequest request, Guid currentUserId ,CancellationToken token)
    {
        var validation = await _createValidator.ValidateAsync(request, token);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        Equipment newEquipment = new Equipment()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            PlacementType = request.PlacementType,

            Status = EquipmentStatus.Available,
            RequiredUserLevel = request.RequiredUserLevel,

            IsDeleted = false,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        
        await _unitOfWork.Equipments.AddAsync(newEquipment, token);
        await _unitOfWork.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(newEquipment);
    }

    public async Task<Response> UpdateAsync(UpdateRequest request, Guid currentUserId, CancellationToken token)
    {
        var validation = _updateValidator.Validate(request);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        var dbEquipment = _unitOfWork.Equipments.Query().FirstOrDefault(x => x.Id == request.Id);

        if (dbEquipment is null)
            throw new NotFoundException(nameof(Equipment), request.Id);

        _mapper.Map(request, dbEquipment);

        /*
        if (request.UsingById.HasValue)
        {
            if (request.UsingById.Value is { } newUserId)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(newUserId, token);
                if (user is null)
                    throw new NotFoundException(nameof(User), newUserId);

                dbEquipment.UsingById = newUserId;
            }
            else
            {
                // Release: kimse kullanmıyor
                dbEquipment.UsingById = null;
                dbEquipment.Status = EquipmentStatus.Available;
            }
        }
        */

        dbEquipment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Equipments.Update(dbEquipment);
        await _unitOfWork.SaveChangesAsync(token);

        return _mapper.Map<Response>(dbEquipment);
    }
    
    //Admin methodu zaten
    public async Task<Response> DeleteAsync(Guid id, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        result.IsDeleted = true;
        
        _unitOfWork.Equipments.Update(result);
        
        await _unitOfWork.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(result);
        
    }

    public async Task<Response> RentAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
{
        var equipment = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (equipment is null)
            throw new NotFoundException(nameof(Equipment), id);

        if (equipment.Status != EquipmentStatus.Available) 
            throw new EquipmentNotAvailableException();

        if (equipment.PlacementType != EquipmentPlacementType.Portable)
            throw new EquipmentNotPortableException();

        var currentUser = GetCurrentUser(currentUserId);
        
        if (currentUser.EquipmentLevel < equipment.RequiredUserLevel)
        {
            throw new InsufficientEquipmentLevelException($"Bu ekipman için gereken seviye: {equipment.RequiredUserLevel}, senin seviyen: {currentUser.EquipmentLevel}.");
        }
        
        var activerentcount = await _unitOfWork.EquipmentRentals.GetActiveRentalCountByUserAsync(currentUserId, token);
        
        if ( activerentcount > 
             _membershipStrategy.CalculateMaximumEquipmentCount()-1 )
            throw new RentalLimitExceededException($"En fazla {_membershipStrategy.CalculateMaximumEquipmentCount()} aktif kiralamanız olabilir, şu an {activerentcount} tane var.");
    
        // ---- burda logic işlemler sonlanıyor artık ---- 
        
        var rental = new EquipmentRental
        {
            UserId = currentUserId,
            EquipmentId = id,
            RentedAt = DateTime.UtcNow,
            ExpectedReturnAt = DateTime.UtcNow + span
        };
        
        var state = _stateFactory.Create(equipment.Status);
        
        
        await state.RentedAsync(equipment, rental);


        _unitOfWork.Equipments.Update(equipment);
        _unitOfWork.EquipmentRentals.AddAsync(rental, token);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<Response>(equipment);

    }

    public async Task<Response> ReserveAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
    {

        var equipment = await _unitOfWork.Equipments.GetByIdAsync(id, token);

        if (equipment is null)
            throw new NotFoundException(nameof(Equipment), id);

        if (equipment.Status != EquipmentStatus.Available)
            throw new EquipmentNotAvailableException();

        if (equipment.PlacementType != EquipmentPlacementType.Benchtop && equipment.PlacementType != EquipmentPlacementType.FloorStationary)
            throw new EquipmentNotPortableException();
        
        var currentUser = GetCurrentUser(currentUserId);
        
        if (currentUser.EquipmentLevel < equipment.RequiredUserLevel)
        {
            throw new InsufficientEquipmentLevelException($"Bu ekipman için gereken seviye: {equipment.RequiredUserLevel}, senin seviyen: {currentUser.EquipmentLevel}.");
        }
        
        var activereservecount = await _unitOfWork.EquipmentRentals.GetActiveRentalCountByUserAsync(currentUserId, token);
        
        if ( activereservecount > 
             _membershipStrategy.CalculateMaximumEquipmentCount()-1 )
            throw new RentalLimitExceededException($"En fazla {_membershipStrategy.CalculateMaximumEquipmentCount()} aktif kiralamanız olabilir, şu an {activereservecount} tane var.");
        
        // -- logic işlemler bitti burada
        
        var rental = new EquipmentRental
        {
            UserId = currentUserId,
            EquipmentId = id,
            RentedAt = DateTime.UtcNow,
            ExpectedReturnAt = DateTime.UtcNow + span
        };

        var state = _stateFactory.Create(equipment.Status);
        await state.ReservedAsync(equipment, rental);
        
        
        _unitOfWork.Equipments.Update(equipment);
        
        _unitOfWork.EquipmentRentals.AddAsync(rental);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<Response>(equipment);
    }
    
    public async Task<Response> ReleaseItAsync(Guid id, Guid currentUserId, CancellationToken token)
    {
        var equipment = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (equipment is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        var activeRental = await _unitOfWork.EquipmentRentals.GetActiveByEquipmentIdAsync(id, token);
        
        if (activeRental is null)
            throw new EquipmentNotRentedException();

        if (activeRental.UserId != currentUserId)
            throw new NotResourceOwnerException($"Bu ekipman senin tarafından kiralanmamış.Ekipmanın =>{ activeRental.UserId}, Senin => {currentUserId} ");
        
        var state = _stateFactory.Create(equipment.Status);
        await state.AvailableAsync(equipment, activeRental);
        
        _unitOfWork.Equipments.Update(equipment);
        _unitOfWork.EquipmentRentals.Update(activeRental);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation($"Released equipment {equipment.Id} by {currentUserId}");
            
        return _mapper.Map<Response>(equipment);
    }

    public async Task<Response> SetMaintenanceAsync(Guid id, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);

        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);

        var state = _stateFactory.Create(result.Status);
        await state.MaintenanceAsync(result);
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync();

        var activeRental = await _unitOfWork.EquipmentRentals.GetActiveRentalAsync(id, token);

        if (activeRental is not null)
        {
            activeRental.ReleasedAt = DateTime.UtcNow;
            _unitOfWork.EquipmentRentals.Update(activeRental);
        }
        result.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync(token);

        return _mapper.Map<Response>(result);
    }
    
    
    public async Task<Response> UnsetMaintenanceAsync(Guid id, CancellationToken token)
    {
        var equipment = await _unitOfWork.Equipments.GetByIdAsync(id, token);

        if (equipment is null)
            throw new NotFoundException(nameof(Equipment), id);

        equipment.Status = EquipmentStatus.Available;
        equipment.UpdatedAt  = DateTime.UtcNow;
        
        _unitOfWork.Equipments.Update(equipment);
        await _unitOfWork.SaveChangesAsync(token);

        return _mapper.Map<Response>(equipment);
    }

    public async Task<PagedResponse<EquipmentRentalResponse>> MyEquipmentsAsync(
        Guid userId, ListRequest request, bool includePast, CancellationToken token)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.EquipmentRentals
            .Query()
            .Include("Equipment")
            .Where(r => r.UserId == userId);

        // ShowReleased false ise sadece aktif kiralıkları göster
        if (!includePast)
        {
            query = query.Where(r => r.ReleasedAt == null);
        }

        var totalCount = await query.CountAsync(token);

        var items = await query
            .OrderByDescending(r => r.RentedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<EquipmentRentalResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(token);

        _logger.LogInformation("Listed {Count}/{Total} rentals for user {UserId} (page {Page}, showReleased={ShowReleased})",
            items.Count, totalCount, userId, page, includePast);

        return new PagedResponse<EquipmentRentalResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private IEquipmentState GetStateFor(EquipmentStatus status)
    {
        return status switch
        {
            EquipmentStatus.Reserved => new ReservedState(),
            EquipmentStatus.Available => new AvailableState(),
            EquipmentStatus.Rented => new RentedState(),
            EquipmentStatus.Maintenance => new MaintenanceState(),
            _ => throw new InvalidOperationException($"Unknown content status {status}")
        };
    }

    private User GetCurrentUser(Guid userId)
    {
        var result =  _unitOfWork.Users.Query().FirstOrDefault(u => u.Id == userId);
        return result ?? throw new NotFoundException(nameof(User), userId);
    }


}