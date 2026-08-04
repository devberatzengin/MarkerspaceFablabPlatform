using MakerspaceFablabPlatform.Dtos.Subscription;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICurrentUserService _currentUserService;

    public SubscriptionController(ISubscriptionService subscriptionService, ICurrentUserService currentUserService)
    {
        _subscriptionService = subscriptionService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<Response>> Create(CreateRequest createRequest, CancellationToken token)
    {
        var result = await _subscriptionService.CreateAsync(createRequest, _currentUserService.GetCurrentUserId(), token);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<Response>>> GetMine(CancellationToken token)
    {
        var result = await _subscriptionService.GetMineAsync(_currentUserService.GetCurrentUserId(), token);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken token)
    {
        var result = await _subscriptionService.DeleteAsync(id, _currentUserService.GetCurrentUserId(), token);

        return Ok(result);
    }
}
