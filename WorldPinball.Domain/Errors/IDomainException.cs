namespace WorldPinball.Domain.Errors;

public interface IDomainException
{
    string ErrorCode { get; }
}
