namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class SilverMembershipStrategy : IMembershipStrategy
{
    public int CalculateMembershipCost()
    {
        return 50;
    }

    public int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (int)releaseOverTime.TotalDays * 50;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 5;
    }
}