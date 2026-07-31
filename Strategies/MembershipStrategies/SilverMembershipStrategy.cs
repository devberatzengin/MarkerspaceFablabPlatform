using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class SilverMembershipStrategy : IMembershipStrategy
{
    public decimal CalculateMembershipCost()
    {
        return 50;
    }

    public decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (decimal)releaseOverTime.TotalHours * 50;
    }
    
    public decimal CalculateDiscountAmount(decimal totalAmount)
    {
        return (decimal)totalAmount * 10 / 100;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 5;
    }
}