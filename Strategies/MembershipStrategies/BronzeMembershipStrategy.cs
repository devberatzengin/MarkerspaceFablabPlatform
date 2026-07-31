namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class BronzeMembershipStrategy : IMembershipStrategy
{
    public decimal CalculateMembershipCost()
    {
        return 10;
    }
    
    public decimal CalculateDiscountAmount(decimal totalAmount)
    {
        return (decimal)((totalAmount * 5) / 100);
    }
    
    public decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (decimal)releaseOverTime.TotalHours* 75;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 3;
    }
}