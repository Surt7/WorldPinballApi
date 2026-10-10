namespace WorldPinball.Logistics.Domain.Entities;

public class OutboxMessage
{
    
        private static class ErrorMessages
        {
            public const string EmptyId = "Message identifier cannot be empty.";
            public const string EmptyType = "Message event type cannot be empty.";
            public const string EmptyContent = "Message payload content cannot be empty.";
            public const string EmptyFailure = "Message failure cannot be empty.";
        }
        private OutboxMessage() { } // 

        public OutboxMessage(Guid id, string type, string content, DateTime occuredAtUtc)
        {
            
            if (id == Guid.Empty)
                throw new ArgumentException(ErrorMessages.EmptyId, nameof(id));
            
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException(ErrorMessages.EmptyType, nameof(type));
            
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException(ErrorMessages.EmptyContent, nameof(content));
            
            
            Id = id;
            Type = type.Trim();
            Content = content;
            OccurredAtUtc = occuredAtUtc;
            
        }

        public string Type { get; private set; } = null!;

        public string Content { get; private set; } = null!;

        public Guid Id { get; private set; }

        public DateTime OccurredAtUtc { get; private set; }
        
        public DateTime? ProcessedAtUtc { get; private set; }
        
        public string? Error { get; private set; }
        
        public int RetryCount { get; private set; }
        
        
        public void MarkAsProcessed(DateTime processedAt)
        {
            ProcessedAtUtc = processedAt;
            Error = null;
        }
        
        public void RecordFailure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException(ErrorMessages.EmptyFailure, nameof(error));
            
            Error = error.Trim();
            RetryCount++;
        }
}