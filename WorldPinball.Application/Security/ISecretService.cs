namespace WorldPinball.Application.Security;

public interface ISecretService
{
    SecretGenerationResult Generate();
}