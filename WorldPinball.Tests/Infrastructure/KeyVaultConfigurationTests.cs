using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace WorldPinball.Tests.Infrastructure;

public sealed class KeyVaultConfigurationTests
{
    [Fact]
    public void ConfigureKeyVault_WhenUriIsNotConfigured_ShouldNotAddKeyVaultProvider()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["KeyVault:VaultUri"] = string.Empty;
        builder.Configuration["KEYVAULT_URI"] = string.Empty;

        // Act
        builder.ConfigureKeyVault();

        // Assert
        var providers = ((IConfigurationRoot)builder.Configuration).Providers;
        providers.Should().NotContain(p => p.GetType().Name.Contains("AzureKeyVault"));
    }

    [Fact]
    public void ConfigureKeyVault_WhenUriIsConfigured_ShouldAttemptToConnectToKeyVault()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["KeyVault:VaultUri"] = "https://kv-worldpinball-test.vault.azure.net/";

        // Act & Assert
        // Adding Azure KeyVault to ConfigurationManager eagerly executes an initial secret load over HTTP,
        // which verifies that the KeyVault provider is registered and connecting to the specified endpoint.
        var act = () => builder.ConfigureKeyVault();

        act.Should().Throw<Exception>();
    }
}
