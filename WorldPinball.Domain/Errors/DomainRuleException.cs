namespace WorldPinball.Domain.Errors;

public sealed class DomainRuleException(string errorCode, string message)
    : InvalidOperationException(message), IDomainException
{
    public string ErrorCode { get; } = errorCode;
}
