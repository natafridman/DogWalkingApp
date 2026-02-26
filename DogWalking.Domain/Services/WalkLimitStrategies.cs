using DogWalking.Domain.Enums;

namespace DogWalking.Domain.Services;

/// <summary>Free tier: 1 walk per month, max 1 per day.</summary>
public sealed class FreeWalkLimitStrategy : WalkLimitStrategyBase
{
    public override SubscriptionType SubscriptionType => SubscriptionType.Free;
    public override int MaxWalksPerMonth => 1;
    public override bool AllowsMultiplePerDay => false;
    public override string Description => "Free — 1 walk/month, max 1/day";
}

/// <summary>Basic tier: 12 walks per month, max 1 per day.</summary>
public sealed class BasicWalkLimitStrategy : WalkLimitStrategyBase
{
    public override SubscriptionType SubscriptionType => SubscriptionType.Basic;
    public override int MaxWalksPerMonth => 12;
    public override bool AllowsMultiplePerDay => false;
    public override string Description => "Basic — 12 walks/month, max 1/day";
}

/// <summary>Pro tier: 20 walks per month, max 1 per day.</summary>
public sealed class ProWalkLimitStrategy : WalkLimitStrategyBase
{
    public override SubscriptionType SubscriptionType => SubscriptionType.Pro;
    public override int MaxWalksPerMonth => 20;
    public override bool AllowsMultiplePerDay => false;
    public override string Description => "Pro — 20 walks/month, max 1/day";
}

/// <summary>Premium tier: 50 walks per month, multiple per day allowed.</summary>
public sealed class PremiumWalkLimitStrategy : WalkLimitStrategyBase
{
    public override SubscriptionType SubscriptionType => SubscriptionType.Premium;
    public override int MaxWalksPerMonth => 50;
    public override bool AllowsMultiplePerDay => true;
    public override string Description => "Premium — 50 walks/month, unlimited/day";
}
