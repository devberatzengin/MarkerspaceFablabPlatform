using AutoMapper;
using AutoMapper.QueryableExtensions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Notification;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakerspaceFablabPlatform.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResponse<Response>> GetMineAsync(Guid currentUserId, ListRequest request, CancellationToken token = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == currentUserId);

        if (request.OnlyUnread)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync(token);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<Response>(_mapper.ConfigurationProvider)
            .ToListAsync(token);

        return new PagedResponse<Response>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Response> MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken token = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdForUserAsync(notificationId, currentUserId, token);

        if (notification is null)
            throw new NotFoundException(nameof(Notification), notificationId);

        if (notification.IsRead)
            return _mapper.Map<Response>(notification);

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(token);

        _logger.LogInformation("Notification marked as read. NotificationId: {NotificationId}, UserId: {UserId}", notificationId, currentUserId);

        return _mapper.Map<Response>(notification);
    }

    public Task<int> GetUnreadCountAsync(Guid currentUserId, CancellationToken token = default)
    {
        return _unitOfWork.Notifications.GetUnreadCountAsync(currentUserId, token);
    }
}
