using System.ComponentModel.DataAnnotations;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Dtos.User;

public class UpdateRequest
{
    
    public string? FirstName { get; set; } = string.Empty;
    
    public string? LastName { get; set; } = string.Empty;
    
    public string? Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; } = string.Empty;
    
    public MembershipStatus Status { get; set; } =  MembershipStatus.Unknown;
}