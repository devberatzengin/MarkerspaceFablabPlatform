using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MakerspaceFablabPlatform.Dtos.Announcement;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }
    
    
    
    // List All Announcement
    [HttpGet]
    public async Task<ActionResult<PagedResponse<Response>>> GetAll([FromQuery] ListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _announcementService.GetAllAsync(request, IsAdmin(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Response?>> GetById(Guid id)
    {
        var result = await _announcementService.GetByIdAsync(id, IsAdmin());
        return Ok(result);
    }

    //  New Announcement
    [ProducesResponseType(typeof(Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult<Response>> Create(CreateRequest request)
    {
        
        var reuslt = await _announcementService.CreateAsync(request, GetCurrentUserId());
        return CreatedAtAction(
            nameof(GetById),
            new { id = reuslt.Id },
            reuslt);
    }
    
    
    // [HttpPut] Edit Announcement By Id
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Response?>> Update(Guid id,UpdateRequest request)
    {
        request.Id = id; // burda ne yaptım bilmiyorum bi an mantığıma yatmadı
        bool isAdmin = User.Claims.Any(c => 
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "Admin");
        var result = await _announcementService.UpdateAsync(request, GetCurrentUserId(), isAdmin);
        return Ok(result);
    }

    // Announcement Publish By Id
    [HttpPatch("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Publish(Guid id)
    {
        var result = await _announcementService.PublishAsync(id, GetCurrentUserId());
        return Ok(result);
    }

    // Announcement UnPublish By Id
    [HttpPatch("{id:guid}/unpublish")] 
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Unpublish(Guid id)
    {
        var result = await _announcementService.UnpublishAsync(id, GetCurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{id:guid}/archive")] // arşivleme silme gibi şuan elle açmadıkça arşivde duruyor şuan
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Archive(Guid id)
    {
        var result = await _announcementService.ArchiveAsync(id, GetCurrentUserId());
        return Ok(result);
    }
    
    
    
    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("Token'da kullanıcı kimliği bulunamadı.");
        return Guid.Parse(value);
    }

    private bool IsAdmin()
    {
        // Token içerisindeki uzun URI'ye sahip claim'i veya direkt ClaimTypes.Role'ü manuel ararız
        return User.Claims.Any(c => 
            (c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" || c.Type == System.Security.Claims.ClaimTypes.Role) 
            && c.Value == "Admin");
    }
}
