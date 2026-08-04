using AutoMapper;
using FluentValidation;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Subscription;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IValidator<CreateRequest> _createValidator;

    public SubscriptionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SubscriptionService> logger,IValidator<CreateRequest> createValidator) {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
    }

    public async Task<Response> CreateAsync(CreateRequest request, Guid currentUserId, CancellationToken token = default)
    {
        var validation = await _createValidator.ValidateAsync(request, token);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        await EnsureTargetExistsAsync(request, token);

        var existing = await _unitOfWork.Subscriptions.Query(asNoTracking: false)
            .FirstOrDefaultAsync(s =>
                s.UserId == currentUserId &&
                s.CategoryId == request.CategoryId &&
                s.EquipmentId == request.EquipmentId, token);

        if (existing is not null)
        {
            if (existing.IsActive && existing.Channel == request.Channel)
                throw new DuplicateEntityException("You already following this thing.");

            existing.IsActive = true;
            existing.Channel = request.Channel;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Subscriptions.Update(existing);
            await _unitOfWork.SaveChangesAsync(token);

            _logger.LogInformation("Update, SubscriptionId: {SubscriptionId}, UserId: {UserId}", existing.Id, currentUserId);

            return _mapper.Map<Response>(existing);
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            TargetType = request.TargetType,
            CategoryId = request.CategoryId,
            EquipmentId = request.EquipmentId,
            Channel = request.Channel,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Subscriptions.AddAsync(subscription, token);
        await _unitOfWork.SaveChangesAsync(token);

        _logger.LogInformation("Create ,SubscriptionId: {SubscriptionId}, UserId: {UserId}, TargetType: {TargetType}",
            subscription.Id, currentUserId, subscription.TargetType);

        return _mapper.Map<Response>(subscription);
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId, Guid currentUserId, CancellationToken token = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdForUserAsync(subscriptionId, currentUserId, token);

        if (subscription is null)
            throw new NotFoundException(nameof(Subscription), subscriptionId);

        subscription.IsActive = false;
        subscription.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.SaveChangesAsync(token);

        _logger.LogInformation("Delete ,SubscriptionId: {SubscriptionId}, UserId: {UserId}", subscriptionId, currentUserId);

        return true;
    }

    public async Task<List<Response>> GetMineAsync(Guid currentUserId, CancellationToken token = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetByUserIdAsync(currentUserId, token);

        return _mapper.Map<List<Response>>(subscriptions);
    }

    private async Task EnsureTargetExistsAsync(CreateRequest request, CancellationToken token)
    {
        if (request.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value, token);

            if (category is null)
                throw new NotFoundException(nameof(Category), request.CategoryId.Value);

            return;
        }

        var equipment = await _unitOfWork.Equipments.GetByIdAsync(request.EquipmentId!.Value, token);

        if (equipment is null)
            throw new NotFoundException(nameof(Equipment), request.EquipmentId.Value);
    }
}
