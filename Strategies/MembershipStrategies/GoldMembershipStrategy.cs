namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class GoldMembershipStrategy : IMembershipStrategy
{
    public decimal CalculateMembershipCost()
    {
        return 100;
    }
    
    public decimal CalculateDiscountAmount(decimal totalAmount)
    {
        return (decimal)((totalAmount * 20) / 100);
    }

    public decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (decimal)releaseOverTime.TotalHours * 25;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 10;
    }
}