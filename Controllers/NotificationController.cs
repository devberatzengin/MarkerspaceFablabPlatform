using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Notification;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Response>>> GetMine([FromQuery] ListRequest request, CancellationToken token)
    {
        var result = await _notificationService.GetMineAsync(_currentUserService.GetCurrentUserId(), request, token);

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken token)
    {
        var result = await _notificationService.GetUnreadCountAsync(_currentUserService.GetCurrentUserId(), token);

        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<Response>> MarkAsRead(Guid id, CancellationToken token)
    {
        var result = await _notificationService.MarkAsReadAsync(id, _currentUserService.GetCurrentUserId(), token);

        return Ok(result);
    }
}
