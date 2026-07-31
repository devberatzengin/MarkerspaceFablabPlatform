using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.User;

public class UserResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserType Type { get; set; } = UserType.User;
    public short EquipmentLevel { get; set; } = 1;
    public decimal Balance { get; set; } = -1; // HATA OLDUĞUNU ANLARIZ
    public MembershipStatus Status { get; set; } = MembershipStatus.Unknown;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
}