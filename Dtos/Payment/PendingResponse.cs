namespace MakerspaceFablabPlatform.Dtos.Payment;


public class PendingResponse
{
    public Guid EquipmentRentalId { get; set; }

    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;

    public DateTime RentedAt { get; set; }
    public DateTime ExpectedReturnAt { get; set; }
    public DateTime ReleasedAt { get; set; }

    public bool IsOverdue { get; set; }
    public TimeSpan? OverdueBy { get; set; }

    public decimal RentalFee { get; set; }
    public decimal LateFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal UserBalance { get; set; }

    public bool HasSufficientBalance => UserBalance >= TotalAmount;
}
