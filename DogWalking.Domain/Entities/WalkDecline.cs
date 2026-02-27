namespace DogWalking.Domain.Entities;

/// <summary>
/// Records that a specific walker declined a walk request.
/// Used to filter out walks a walker has already seen and passed on.
/// </summary>
public class WalkDecline
{
    public int      Id          { get; private set; }
    public int      WalkEventId { get; private set; }
    public int      WalkerId    { get; private set; }
    public DateTime DeclinedAt  { get; private set; }

    public WalkEvent WalkEvent { get; private set; } = null!;
    public User      Walker    { get; private set; } = null!;

    private WalkDecline() { }

    public WalkDecline(int walkerId)
    {
        WalkerId   = walkerId;
        DeclinedAt = DateTime.UtcNow;
    }
}
