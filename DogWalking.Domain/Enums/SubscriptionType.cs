namespace DogWalking.Domain.Enums;

/// <summary>
/// Subscription tiers for dog walking clients.
///
/// Free    : 1  walk / month,  max 1/day.
/// Basic   : 12 walks / month, max 1/day.
/// Pro     : 20 walks / month, max 1/day.
/// Premium : 50 walks / month, unlimited per day.
/// </summary>
public enum SubscriptionType
{
    Free    = 1,
    Basic   = 2,
    Pro     = 3,
    Premium = 4
}
