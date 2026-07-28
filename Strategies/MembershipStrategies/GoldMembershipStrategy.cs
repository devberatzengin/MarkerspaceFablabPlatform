namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class GoldMembershipStrategy : IMembershipStrategy
{
    public int CalculateMembershipCost()
    {
        return 100;
    }

    public int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (int)releaseOverTime.TotalDays * 50;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 10;
    }
}