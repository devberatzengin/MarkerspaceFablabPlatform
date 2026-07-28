namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class ProfessionalMembershipStrategy : IMembershipStrategy
{
    public int CalculateMembershipCost()
    {
        return 1000;
    }

    public int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (int)releaseOverTime.TotalDays * 25;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 20;
    }
}