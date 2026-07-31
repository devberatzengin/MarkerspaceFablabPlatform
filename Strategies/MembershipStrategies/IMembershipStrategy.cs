using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public interface IMembershipStrategy
{
    decimal CalculateMembershipCost();
    
    decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime); // Late'i Hesapladı
    
    
    decimal CalculateDiscountAmount(decimal totalAmountWithoutDiscount);
    
    int CalculateMaximumEquipmentCount();
    
}