using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.Payment;

public class ListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public PaymentStatus? Status { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public string? Search { get; set; }
}
