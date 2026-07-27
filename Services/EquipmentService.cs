using AutoMapper;
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

    public EquipmentService(IMapper mapper,IUnitOfWork unitOfWork,IEquipmentRepository equipmentRepository, ILogger<EquipmentService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
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

        return ToResponse(result);

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
            .Select(e => new Response
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Type = e.Type,
                PlacementType = e.PlacementType,
                Status = e.Status,
                RequiredUserLevel = e.RequiredUserLevel,
                IsDeleted = e.IsDeleted,
                UsingById = e.UsingById,
                AvailableAt = e.AvailableAt
            })
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

            UsingById = null,  

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AvailableAt = DateTime.UtcNow
        };
        
         _unitOfWork.Equipments.AddAsync(newEquipment, token);
        await _unitOfWork.SaveChangesAsync(token);
        
        return ToResponse(newEquipment);
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

        return ToResponse(dbEquipment);
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
        
        return ToResponse(result);
        
    }

    public async Task<Response> RentAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        if (result.Status != EquipmentStatus.Available)
            throw new Exception($"Equipment {result.Id} is not available");

        if (result.PlacementType  != EquipmentPlacementType.Portable)
            throw new Exception($"Equipment {result.Id} is not portable. You van just reserve it");


        var state = GetStateFor(result.Status); 
        await state.RentedAsync(result,_unitOfWork);

        result.UsingById = currentUserId;
        result.AvailableAt = DateTime.UtcNow + span;
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.Equipments.SaveChangesAsync(token);
        
        return ToResponse(result);
        
    }

    public async Task<Response> ReserveAsync(Guid id, TimeSpan span, Guid currentUserId, CancellationToken token)
    {
        
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        if (result.Status != EquipmentStatus.Available)
            throw new Exception($"Equipment {result.Id} is not available");

        if (result.PlacementType  == EquipmentPlacementType.Portable)
            throw new Exception($"Equipment {result.Id} is portable. You can just rent it not reserve it");


        var state =  GetStateFor(result.Status); 
        await state.ReservedAsync(result,_unitOfWork);

        result.UsingById = currentUserId;
        result.AvailableAt = DateTime.UtcNow + span;
        
        _unitOfWork.Equipments.Update(result);
        await _unitOfWork.Equipments.SaveChangesAsync(token);
        
        return ToResponse(result);
        
    }
    
    public async Task<Response> ReleaseItAsync(Guid id, Guid currentUserId, CancellationToken token)
    {
        var result = await _unitOfWork.Equipments.GetByIdAsync(id, token);
        
        if (result is null)
            throw new NotFoundException(nameof(Equipment), id);
        
        if (result.UsingById is null)
            throw new UnauthorizedAccessException("This equipment is not yet used by someone");
        
        if (result.UsingById != currentUserId)
            throw new UnauthorizedAccessException("It's not your equipment");

        
        var state = GetStateFor(result.Status);
        await state.AvailableAsync(result,_unitOfWork);
        result.UsingById = null;
        
        result.AvailableAt = DateTime.UtcNow.AddHours(1);
        
        _unitOfWork.Equipments.Update(result);
        
        await _unitOfWork.Equipments.SaveChangesAsync(token);
            
        return ToResponse(result);
    }

    
    
    private static Response ToResponse(Equipment equipment) => new()
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Description = equipment.Description,
        Type = equipment.Type,
        PlacementType = equipment.PlacementType,
        Status = equipment.Status,
        RequiredUserLevel = equipment.RequiredUserLevel,
        IsDeleted = equipment.IsDeleted,
        UsingById = equipment.UsingById,
        AvailableAt = equipment.AvailableAt
    };
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