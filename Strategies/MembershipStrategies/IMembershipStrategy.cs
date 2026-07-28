namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public interface IMembershipStrategy
{
    int CalculateMembershipCost();
    int CalculateReleaseOverTimeCost(TimeSpan releaseOverTime);
    int CalculateMaximumEquipmentCount();
}