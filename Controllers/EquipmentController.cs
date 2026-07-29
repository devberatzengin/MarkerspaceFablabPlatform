using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Helpers;
using Response = MakerspaceFablabPlatform.Dtos.Equipment.Response;
using MakerspaceFablabPlatform.Dtos.Equipment;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EquipmentRentalResponse = MakerspaceFablabPlatform.Dtos.EquipmentRental.Response;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        var result =  await _equipmentService.CreateAsync(request, User.GetCurrentUserId(), token);
        
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> UpdateAsync(UpdateRequest request, CancellationToken token)
    {
        var result = await _equipmentService.UpdateAsync(request, User.GetCurrentUserId(), token);
        
        return Ok(result);
        
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> DeleteAsync(Guid id, CancellationToken token)
    {
        var response = await _equipmentService.DeleteAsync(id, token);
        return Ok(response);
    }


    [HttpPatch("{id:guid}/rent")]
    public async Task<ActionResult<Response>> RentAsync(Guid id, TimeSpan span, CancellationToken token)
    {
        var result = await _equipmentService.RentAsync(id, span, User.GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/reserve")]
    public async Task<ActionResult<Response>> ReserveAsync(Guid id, TimeSpan span, CancellationToken token)
    {
        var result = await _equipmentService.ReserveAsync(id, span, User.GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/release")]
    public async Task<ActionResult<Response>> ReleaseItAsync(Guid id, CancellationToken token)
    {
        var result = await _equipmentService.ReleaseItAsync(id, currentUserId:User.GetCurrentUserId(), token);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/maintenance")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> SetMaintenanceAsync(Guid id, CancellationToken token)
    {
        var result = await _equipmentService.SetMaintenanceAsync(id, token);
        return Ok(result);
    }

    [HttpGet("my-equipments")]
    public async Task<ActionResult<PagedResponse<EquipmentRentalResponse>>> MyEquipmentsAsync(
        [FromQuery] ListRequest request,
        CancellationToken token,
        [FromQuery]bool includePast = false)
    {
        var result = await _equipmentService.MyEquipmentsAsync(User.GetCurrentUserId(), request, includePast, token);
        return Ok(result);
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