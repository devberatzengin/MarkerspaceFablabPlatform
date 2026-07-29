namespace MakerspaceFablabPlatform.Dtos.EquipmentRental;

public class Response
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid EquipmentId { get; set; }

    public string EquipmentName { get; set; } = string.Empty;

    public string EquipmentDescription { get; set; } = string.Empty;

    public DateTime RentedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public DateTime ExpectedReturnAt { get; set; }

    public bool IsActive => ReleasedAt == null;
}
