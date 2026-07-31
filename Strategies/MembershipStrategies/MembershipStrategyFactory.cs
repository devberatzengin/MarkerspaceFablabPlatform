using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Strategies.MembershipStrategies;

public interface IMembershipStrategyFactory
{
    IMembershipStrategy Create(MembershipStatus status);
}

public class MembershipStrategyFactory : IMembershipStrategyFactory
{
    public IMembershipStrategy Create(MembershipStatus status) 
        => status switch
            {
                MembershipStatus.Free => new FreeMembershipStrategy(),
                MembershipStatus.Bronze => new BronzeMembershipStrategy(),
                MembershipStatus.Silver => new SilverMembershipStrategy(),
                MembershipStatus.Gold => new GoldMembershipStrategy(),
                MembershipStatus.Professional => new ProfessionalMembershipStrategy(),
                _ => new FreeMembershipStrategy()
            };
}
