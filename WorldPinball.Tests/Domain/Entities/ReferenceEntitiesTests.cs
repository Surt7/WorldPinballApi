using FluentAssertions;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Domain.Entities;

public class ReferenceEntitiesTests
{
    // ===================================
    // GameMode Tests
    // ===================================
    [Fact]
    public void GameMode_Constructor_WhenValid_ShouldInitializeProperly()
    {
        var mode = new GameMode("classic", "Classic Mode", "Standard pinball mode");

        mode.ModeId.Should().NotBeEmpty();
        mode.Code.Should().Be("classic");
        mode.Name.Should().Be("Classic Mode");
        mode.Description.Should().Be("Standard pinball mode");
        mode.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GameMode_Constructor_WhenCodeInvalid_ThrowsArgumentException(string? invalidCode)
    {
        Action act = () => new GameMode(invalidCode!, "Classic");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GameMode_Constructor_WhenNameInvalid_ThrowsArgumentException(string? invalidName)
    {
        Action act = () => new GameMode("code", invalidName!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GameMode_ActivateAndDeactivate_ShouldUpdateStatusAndTimestamp()
    {
        var mode = new GameMode("tourney", "Tournament Mode", isActive: true);
        var now = DateTime.UtcNow;

        mode.Deactivate(now);
        mode.IsActive.Should().BeFalse();
        mode.UpdatedAtUtc.Should().Be(now);

        var resumeTime = now.AddHours(1);
        mode.Activate(resumeTime);
        mode.IsActive.Should().BeTrue();
        mode.UpdatedAtUtc.Should().Be(resumeTime);
    }

    // ===================================
    // Publisher Tests
    // ===================================
    [Fact]
    public void Publisher_Constructor_WhenValid_ShouldInitializeProperly()
    {
        var publisher = new Publisher("Stern Pinball", "stern-pinball");

        publisher.PublisherId.Should().NotBeEmpty();
        publisher.Name.Should().Be("Stern Pinball");
        publisher.Slug.Should().Be("stern-pinball");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Publisher_Constructor_WhenNameInvalid_ThrowsArgumentException(string? invalidName)
    {
        Action act = () => new Publisher(invalidName!, "slug");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Publisher_Constructor_WhenSlugInvalid_ThrowsArgumentException(string? invalidSlug)
    {
        Action act = () => new Publisher("Name", invalidSlug!);
        act.Should().Throw<ArgumentException>();
    }

    // ===================================
    // Region Tests
    // ===================================
    [Fact]
    public void Region_Constructor_WhenValid_ShouldInitializeProperly()
    {
        var region = new Region("eu", "Europe");

        region.RegionId.Should().NotBeEmpty();
        region.Code.Should().Be("EU");
        region.Name.Should().Be("Europe");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Region_Constructor_WhenCodeInvalid_ThrowsArgumentException(string? invalidCode)
    {
        Action act = () => new Region(invalidCode!, "Europe");
        act.Should().Throw<ArgumentException>();
    }

    // ===================================
    // IpLicense Tests
    // ===================================
    [Fact]
    public void IpLicense_Constructor_WhenValid_ShouldInitializeProperly()
    {
        var license = new IpLicense("Godzilla", "godzilla", ipOwner: "Toho", releaseYear: 2021);

        license.IpId.Should().NotBeEmpty();
        license.Name.Should().Be("Godzilla");
        license.Slug.Should().Be("godzilla");
        license.IpOwner.Should().Be("Toho");
        license.ReleaseYear.Should().Be(2021);
        license.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IpLicense_Constructor_WhenNameInvalid_ThrowsArgumentException(string? invalidName)
    {
        Action act = () => new IpLicense(invalidName!, "slug");
        act.Should().Throw<ArgumentException>();
    }

    // ===================================
    // UserProfile Tests
    // ===================================
    [Fact]
    public void UserProfile_Constructor_WhenValid_ShouldInitializeProperly()
    {
        var userId = Guid.NewGuid();
        var profile = new UserProfile(userId, "PinballWizard", countryCode: "fr", isProfilePublic: true);

        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be("PinballWizard");
        profile.CountryCode.Should().Be("FR");
        profile.IsProfilePublic.Should().BeTrue();
    }

    [Fact]
    public void UserProfile_Constructor_WhenUserIdEmpty_ThrowsArgumentException()
    {
        Action act = () => new UserProfile(Guid.Empty, "DisplayName");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UserProfile_Constructor_WhenDisplayNameInvalid_ThrowsArgumentException(string? invalidName)
    {
        Action act = () => new UserProfile(Guid.NewGuid(), invalidName!);
        act.Should().Throw<ArgumentException>();
    }
}
