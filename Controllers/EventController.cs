using System.Security.Claims;
using MarkerspaceFablabPlatform.Dtos.Common;
using MarkerspaceFablabPlatform.Excepitons;
using MarkerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MarkerspaceFablabPlatform.Dtos.Event;
using Microsoft.AspNetCore.Authorization;

namespace MarkerspaceFablabPlatform.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [ProducesResponseType(typeof(Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> Create(CreateRequest createRequest)
    {
        var result = await _eventService.CreateAsync(createRequest, GetCurrentUserId());
        return CreatedAtAction(
            nameof(GetById),
            new { eventId = result.Id },
            result);
    }

    // Liste tüm login yaonlar
    [HttpGet]
    public async Task<ActionResult<PagedResponse<Response>>> GetAll([FromQuery] ListRequest request)
    {
        var result = await _eventService.GetAllAsync(request, IsAdmin());
        return Ok(result);
    }

    // detayları sadece girişler görür
    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<Response?>> GetById(Guid eventId)
    {
        var result = await _eventService.GetByIdAsync(eventId, IsAdmin());
        return Ok(result);
    }


    [HttpPut("{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Update(Guid eventId, UpdateRequest request)
    {
        request.Id = eventId;
        var result = await _eventService.UpdateAsync(request, GetCurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{eventId:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Publish(Guid eventId)
    {
        var result = await _eventService.PublishAsync(eventId, GetCurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{eventId:guid}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Unpublish(Guid eventId)
    {
        var result = await _eventService.UnpublishAsync(eventId, GetCurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{eventId:guid}/archive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Archive(Guid eventId)
    {
        var result = await _eventService.ArchiveAsync(eventId, GetCurrentUserId());
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("Token'da kullanıcı kimliği bulunamadı.");
        return Guid.Parse(value);
    }

    private bool IsAdmin() => User.IsInRole("Admin");
}
