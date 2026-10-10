using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class StreamSubscription
{
    private StreamSubscription() { } // Requis par EF Core

    public Guid SubscriptionId { get; private set; }
    public Guid SubscriberUserId { get; private set; }
    public PrincipalType TargetType { get; private set; }
    public Guid TargetPrincipalId { get; private set; }
    public DateTime SubscribedAtUtc { get; private set; }
    public bool NotificationsEnabled { get; private set; }

    public StreamSubscription(
        Guid subscriberUserId,
        PrincipalType targetType,
        Guid targetPrincipalId,
        DateTime subscribedAtUtc,
        bool notificationsEnabled = true)
    {
        if (subscriberUserId == Guid.Empty)
            throw new ArgumentException("SubscriberUserId cannot be empty.", nameof(subscriberUserId));

        if (targetPrincipalId == Guid.Empty)
            throw new ArgumentException("TargetPrincipalId cannot be empty.", nameof(targetPrincipalId));

        if (subscriberUserId == targetPrincipalId && targetType == PrincipalType.User)
            throw new InvalidOperationException("A user cannot subscribe to themselves.");

        SubscriptionId = Guid.NewGuid();
        SubscriberUserId = subscriberUserId;
        TargetType = targetType;
        TargetPrincipalId = targetPrincipalId;
        SubscribedAtUtc = subscribedAtUtc;
        NotificationsEnabled = notificationsEnabled;
    }

    public void SetNotifications(bool enabled)
    {
        NotificationsEnabled = enabled;
    }
}