using System.Security.Claims;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    
    
    private readonly IEquipmentService _equipmentService;
    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }
    
    [HttpGet("/{id:guid}")]
    public async Task<ActionResult<Response?>> GetByIdAsync(Guid id, CancellationToken token)
    {
       var result = await _equipmentService.GetByIdAsync(id, token);

       return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Response>>> GetAllAsync([FromQuery] ListRequest request, CancellationToken token)
    {
        var result = await _equipmentService.GetAllAsync(request, token);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        var result =  await _equipmentService.CreateAsync(request, GetCurrentUserId(), token);
        
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<Response>> UpdateAsync(UpdateRequest request, CancellationToken token)
    {
        var result = await _equipmentService.UpdateAsync(request, GetCurrentUserId(), token);
        
        return Ok(result);
        
    }

    [HttpDelete]
    public async Task<ActionResult<Response>> DeleteAsync(Guid id, CancellationToken token)
    {
        var response = await _equipmentService.DeleteAsync(id, token);
        return Ok(response);
    }


    [HttpPatch("{id:guid}/rent")]
    public async Task<ActionResult<Response>> RentAsync(Guid id, TimeSpan span, CancellationToken token)
    {
        var result = await _equipmentService.RentAsync(id, span, GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/reserve")]
    public async Task<ActionResult<Response>> ReserveAsync(Guid id, TimeSpan span, CancellationToken token)
    {
        var result = await _equipmentService.ReserveAsync(id, span, GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/release")]
    public async Task<ActionResult<Response>> ReleaseItAsync(Guid id, CancellationToken token)
    {
        var result = await _equipmentService.ReleaseItAsync(id, currentUserId:GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/maintenance")]
    public async Task<ActionResult<Response>> SetMaintenanceAsync(Guid id, CancellationToken token)
    {
        var result = await _equipmentService.SetMaintenanceAsync(id, token);
        return Ok(result);
    }


    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedException("Token'da kullanıcı kimliği bulunamadı.");
        return Guid.Parse(value);
    }

    
    // Get All
    // Get By Id
    // Create
    // Update
    // Delete
    
    
    // Maintenance e sok
    // Rent it
    // Reserve it
    
    // Release it
    
}