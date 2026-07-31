namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public class ProfessionalMembershipStrategy : IMembershipStrategy
{
    public decimal CalculateMembershipCost()
    {
        return 1000;
    }
    
    public decimal CalculateDiscountAmount(decimal totalAmount)
    {
        return (decimal)((totalAmount * 40) / 100);
    }

    public decimal CalculateReleaseOverTimeCost(TimeSpan releaseOverTime)
    {
        return (decimal)releaseOverTime.TotalHours * 0;
    }

    public int CalculateMaximumEquipmentCount()
    {
        return 20;
    }
}