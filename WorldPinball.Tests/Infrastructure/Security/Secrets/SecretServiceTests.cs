using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using NSubstitute;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Infrastructure.Security.Secrets;

namespace WorldPinball.Tests.Infrastructure.Security.Secrets;

public class SecretServiceTests
{
    private readonly SecretService _sut;

    public SecretServiceTests()
    {
        // EphemeralDataProtectionProvider is an in-memory ephemeral provider from ASP.NET Core,
        // ideal for unit tests: no disk persistence, no persisted keys, 0 fragile mocks.
        var provider = new EphemeralDataProtectionProvider();
        _sut = new SecretService(provider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryHash_WhenSecretIsNullOrWhiteSpace_ReturnsFalseAndNullHash(string? invalidSecret)
    {
        // Act
        var result = _sut.TryHash(invalidSecret!, out var hashHex);

        // Assert
        result.Should().BeFalse();
        hashHex.Should().BeNull();
    }

    [Fact]
    public void TryHash_WhenSecretIsNotValidBase64_ReturnsFalseAndNullHash()
    {
        // Arrange
        const string malformedSecret = "ThisIsNotValidBase64!@#$";

        // Act
        var result = _sut.TryHash(malformedSecret, out var hashHex);

        // Assert
        result.Should().BeFalse();
        hashHex.Should().BeNull();
    }

    [Fact]
    public void TryHash_WhenSecretIsBase64ButNot32Bytes_ReturnsFalseAndNullHash()
    {
        // Arrange: 16 random bytes (128 bits) instead of expected 32 bytes (256 bits)
        var shortSecretBytes = RandomNumberGenerator.GetBytes(16);
        var shortSecretBase64 = Convert.ToBase64String(shortSecretBytes);

        // Act
        var result = _sut.TryHash(shortSecretBase64, out var hashHex);

        // Assert
        result.Should().BeFalse();
        hashHex.Should().BeNull();
    }

 

    [Fact]
    public void Generate_WhenCalled_ReturnsValidSecretAndMatchingHash()
    {
        var result = _sut.Generate();

        byte[] decodedBytes = Convert.FromBase64String(result.PlainBase64);
        var expectedHashHex = Convert.ToHexString(SHA256.HashData(decodedBytes)).ToLowerInvariant();

        decodedBytes.Should().HaveCount(32);
        result.HashHex.Should().Be(expectedHashHex);
    }

    [Fact]
    public void TryHash_WhenSecretMatchesGeneratedSecret_ReturnsTrueAndSameHash()
    {
        
        var result = _sut.Generate();
        
        var plainBase = result.PlainBase64;
        
        var isSuccess = _sut.TryHash(plainBase, out var calculatedHash );

        isSuccess.Should().BeTrue();
        calculatedHash.Should().Be(result.HashHex);
    }
}