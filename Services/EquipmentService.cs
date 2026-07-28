using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using MakerspaceFablabPlatform.States.EquipmentStates;
using Microsoft.EntityFrameworkCore;
using ValidationException = FluentValidation.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class EquipmentService : IEquipmentService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ILogger<EquipmentService> _logger;
    private readonly IMapper _mapper;
    
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;

    public EquipmentService(IMapper mapper, IUnitOfWork unitOfWork, IEquipmentRepository equipmentRepository, ILogger<EquipmentService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _equipmentRepository = equipmentRepository;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    
    public async Task<Response?> GetByIdAsync(Guid id, CancellationToken token)
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

        var query = _unitOfWork.Equipments.Query();

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
            throw new ValidationException(validation.Errors);

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
            AvailableAt = DateTime.UtcNow
        };
        
         _unitOfWork.Equipments.AddAsync(newEquipment, token);
        await _unitOfWork.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(newEquipment);
    }

    public async Task<Response> UpdateAsync(UpdateRequest request, Guid currentUserId, CancellationToken token)
    {
        var validation = _updateValidator.Validate(request);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

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
        await _unitOfWork.Equipments.SaveChangesAsync(token);

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
        
        await _unitOfWork.Equipments.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(result);
        
    }

    public async Task<Response> RentAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        if (result.Status != EquipmentStatus.Available)
            throw new ConflictException($"Equipment {result.Id} is not available");

        if (result.AvailableAt > DateTime.UtcNow)
            throw new ConflictException($"Equipment is not available until {result.AvailableAt:u}");

        if (result.PlacementType  != EquipmentPlacementType.Portable)
            throw new ConflictException($"Equipment {result.Id} is not portable. You can just reserve it");


        var state = GetStateFor(result.Status);
        await state.RentedAsync(result, _unitOfWork);

        _logger.LogInformation($"Renting equipment {result.Id} to {currentUserId}");

        
        var rental = new EquipmentRental
        {
            UserId = currentUserId,
            EquipmentId = result.Id,
            RentedAt = DateTime.UtcNow,
            ExpectedReturnAt = DateTime.UtcNow + span
        };

        await _unitOfWork.EquipmentRentals.AddAsync(rental, token);
        result.AvailableAt = rental.ExpectedReturnAt;
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(result);
    }

    public async Task<Response> ReserveAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        if (result.Status != EquipmentStatus.Available)
            throw new ConflictException($"Equipment {result.Id} is not available");

        if (result.AvailableAt > DateTime.UtcNow)
            throw new ConflictException($"Equipment is not available until {result.AvailableAt:u}");

        if (result.PlacementType == EquipmentPlacementType.Portable)
            throw new ConflictException($"Equipment {result.Id} is portable. You can just rent it not reserve it");

        var state = GetStateFor(result.Status);
        await state.ReservedAsync(result, _unitOfWork);

        _logger.LogInformation($"Reserving equipment {result.Id} to {currentUserId}");
        
        var rental = new EquipmentRental
        {
            UserId = currentUserId,
            EquipmentId = result.Id,
            RentedAt = DateTime.UtcNow,
            ExpectedReturnAt = DateTime.UtcNow + span
        };

        await _unitOfWork.EquipmentRentals.AddAsync(rental, token);
        result.AvailableAt = rental.ExpectedReturnAt;
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync(token);
        
        return _mapper.Map<Response>(result);
    }
    
    public async Task<Response> ReleaseItAsync(Guid id, Guid currentUserId, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        var activeRental = await _unitOfWork.EquipmentRentals.GetActiveRentalAsync(id, token);
        
        if (activeRental is null)
            throw new ConflictException("This equipment is not currently rented");

        if (activeRental.UserId != currentUserId)
            throw new ForbiddenException("This equipment is not rented by you");
        
        var state = GetStateFor(result.Status);
        await state.AvailableAsync(result, _unitOfWork);
        
        activeRental.ReleasedAt = DateTime.UtcNow;
        result.AvailableAt = DateTime.UtcNow.AddHours(1);
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync(token);
        
        _logger.LogInformation($"Released equipment {result.Id} by {currentUserId}");
            
        return _mapper.Map<Response>(result);
    }

    public async Task<Response> SetMaintenanceAsync(Guid id, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);

        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);

        var state = GetStateFor(result.Status);
        await state.MaintenanceAsync(result, _unitOfWork);

        var activeRental = await _unitOfWork.EquipmentRentals.GetActiveRentalAsync(id, token);

        if (activeRental is not null)
            activeRental.ReleasedAt = DateTime.UtcNow;

        result.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.SaveChangesAsync(token);

        return _mapper.Map<Response>(result);
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

}