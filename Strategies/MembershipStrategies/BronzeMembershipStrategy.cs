namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class BronzeMembershipStrategy : IMembershipStrategy
{
    public int CalculateMembershipCost()
    {
        return 10;
    }

    public int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return  (int)releaseOverTime.TotalDays * 75;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 3;
    }
}