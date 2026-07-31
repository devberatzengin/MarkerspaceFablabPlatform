using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Payment;
using MakerspaceFablabPlatform.Helpers;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Response = MakerspaceFablabPlatform.Dtos.Payment.Response;

namespace MakerspaceFablabPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<Response>>> GetAllAsync([FromQuery] ListRequest request, CancellationToken token)
    {
        var result = await _paymentService.GetAllAsync(request, token);
        return Ok(result);
    }

    [HttpGet("my-payments")]
    public async Task<ActionResult<PagedResponse<Response>>> MyPaymentsAsync([FromQuery] ListRequest request, CancellationToken token)
    {
        var result = await _paymentService.GetAllByUserIdAsync(User.GetCurrentUserId(), request, token);
        return Ok(result);
    }

    // Ödenmemiş kiralamalar, hesaplanmış tutarlarıyla birlikte.
    [HttpGet("pending")]
    public async Task<ActionResult<PagedResponse<PendingResponse>>> PendingAsync([FromQuery] ListRequest request, CancellationToken token)
    {
        var result = await _paymentService.GetPendingAsync(User.GetCurrentUserId(), request, token);
        return Ok(result);
    }

    // Ödeme ekranının tutarı göstermesi için; hiçbir şey kaydetmez.
    [HttpGet("preview/{equipmentRentalId:guid}")]
    public async Task<ActionResult<PendingResponse>> PreviewAsync(Guid equipmentRentalId, CancellationToken token)
    {
        var result = await _paymentService.GetPreviewAsync(
            equipmentRentalId, User.GetCurrentUserId(), User.IsAdmin(), token);

        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<Response>>> GetAllByUserIdAsync(
        Guid userId,
        [FromQuery] ListRequest request,
        CancellationToken token)
    {
        var result = await _paymentService.GetAllByUserIdAsync(userId, request, token);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Response>> GetByIdAsync(Guid id, CancellationToken token)
    {
        var result = await _paymentService.GetByIdAsync(id, User.GetCurrentUserId(), User.IsAdmin(), token);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        var result = await _paymentService.CreateAsync(
            request, User.GetCurrentUserId(), User.IsAdmin(), token);

        return Ok(result);
    }
}
