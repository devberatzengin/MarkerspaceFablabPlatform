using System.Threading.Tasks;
using MakerspaceFablabPlatform.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakerspaceFablabPlatform.Dtos.Category;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]                                    
public class CategoryController : ControllerBase
{
    
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    

    [HttpPost]
    [ProducesResponseType(typeof(Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> Create(CreateRequest createRequest)
    {
         var result = await _categoryService.CreateAsync(createRequest);
         
         return CreatedAtAction(
             nameof(GetById),
             new { id = result.Id },
             result);
    }

    [HttpGet]
    public async Task<ActionResult<List<Response>>> GetAll([FromQuery] bool includeUnactivated = false)
    {
        var result = await _categoryService.GetAllAsync(includeUnactivated && IsAdmin());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Response>> GetById(Guid id, [FromQuery] bool includeUnactivated = false)
    {
        var result = await _categoryService.GetByIdAsync(id, includeUnactivated && IsAdmin());
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response?>> Update(UpdateRequest updateRequest)
    {
        var result = await _categoryService.UpdateAsync(updateRequest);
        return Ok(result);
    }
    

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Response>> Deactivate(Guid id)
    {
        var result = await _categoryService.DeactivateAsync(id);
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(Guid categoryId)
    {
        var result = await _categoryService.DeleteAsync(categoryId);
        return Ok(result);
    }

    private bool IsAdmin() => User.IsInRole("Admin");
}