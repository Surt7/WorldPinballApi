using WorldPinball.Domain.Enums;

namespace WorldPinball.Domain.Entities;

public class StreamingAccessRequest
{
    private StreamingAccessRequest()
    {
    } // Used by EF Core

    public Guid RequestId { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public StreamingRequestStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public string? RejectionReason { get; private set; }



    private static class ErrorMessages
    {
        public const string EmptySessionId = "SessionId cannot be empty.";
        public const string EmptyUserId = "UserId cannot be empty.";
        public const string InvalidCreatedAt = "CreatedAtUtc must be a valid date.";
        public const string CannotApproveNonPending = "Cannot approve a request that is not in Pending status.";
        public const string CannotRejectNonPending = "Cannot reject a request that is not in Pending status.";
        public const string EmptyRejectionReason = "Rejection reason cannot be empty.";
        public const string InvalidDuration = "Duration must be greater than zero.";
    }

    public StreamingAccessRequest(
        Guid sessionId,
        Guid requestedByUserId,
        DateTime createdAtUtc
        )
    {
        if (sessionId == Guid.Empty) throw 
            new ArgumentException(ErrorMessages.EmptySessionId,
            nameof(sessionId));
        
        if (requestedByUserId == Guid.Empty) throw 
            new ArgumentException(ErrorMessages.EmptyUserId,
            nameof(requestedByUserId));
        
        if (createdAtUtc == default) throw 
            new ArgumentException(ErrorMessages.InvalidCreatedAt,
            nameof(createdAtUtc));
        
        RequestId = Guid.NewGuid();
        SessionId = sessionId;
        RequestedByUserId = requestedByUserId;
        Status = StreamingRequestStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public void Approve(DateTime approvedAtUtc, TimeSpan duration)
    {
        if (Status is not StreamingRequestStatus.Pending)
            throw new InvalidOperationException(
                ErrorMessages.CannotApproveNonPending);
        
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration),
                ErrorMessages.InvalidDuration);

        // Transition to Approved and calculate lease expiration
        Status = StreamingRequestStatus.Approved;
        ReviewedAtUtc = approvedAtUtc;
        ExpiresAtUtc = approvedAtUtc.Add(duration);
    }

    public void Reject(string reason, DateTime rejectedAtUtc)
    {
        if (Status is not StreamingRequestStatus.Pending)
            throw new InvalidOperationException(ErrorMessages.CannotRejectNonPending);
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(ErrorMessages.EmptyRejectionReason,
                nameof(reason));
        
        Status = StreamingRequestStatus.Rejected;
        ReviewedAtUtc = rejectedAtUtc;
        RejectionReason = reason.Trim();
    }
}