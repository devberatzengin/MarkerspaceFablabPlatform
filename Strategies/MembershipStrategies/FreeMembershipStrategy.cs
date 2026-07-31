using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class FreeMembershipStrategy : IMembershipStrategy
{
    public decimal CalculateMembershipCost()
    {
        return 0;
    }

    public decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (decimal) releaseOverTime.TotalHours * 100;
    }
    
    public decimal CalculateDiscountAmount(decimal totalAmount)
    {
        return 0;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 1;
    }
}