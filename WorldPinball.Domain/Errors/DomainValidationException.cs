namespace WorldPinball.Domain.Errors;

public sealed class DomainValidationException(
    string errorCode,
    string message,
    string parameterName)
    : ArgumentException(message, parameterName), IDomainException
{
    public string ErrorCode { get; } = errorCode;
}
