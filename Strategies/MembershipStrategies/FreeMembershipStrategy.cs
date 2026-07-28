namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class FreeMembershipStrategy : IMembershipStrategy
{
    public int CalculateMembershipCost()
    {
        return 0;
    }

    public int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (int)releaseOverTime.TotalDays * 100;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 1;
    }
}