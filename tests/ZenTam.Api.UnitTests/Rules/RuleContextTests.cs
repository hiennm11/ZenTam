using FluentAssertions;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Rules.Models;

namespace ZenTam.Api.UnitTests.Rules;

public class RuleContextTests
{
    private static UserProfile CreateProfile(
        Gender gender = Gender.Male,
        int lunarYOB = 1990,
        int targetYear = 2025)
    {
        return new UserProfile
        {
            Gender = gender,
            LunarYOB = lunarYOB,
            TargetYear = targetYear
        };
    }

    private static LunarDateContext CreateLunarContext(
        int lunarYear = 2025,
        int lunarMonth = 1,
        int lunarDay = 1,
        bool isLeap = false,
        int jdn = 2460635)
    {
        return new LunarDateContext
        {
            LunarYear = lunarYear,
            LunarMonth = lunarMonth,
            LunarDay = lunarDay,
            IsLeap = isLeap,
            Jdn = jdn
        };
    }

    [Fact]
    public void Constructor_WithRequiredProfileAndLunar_SetsProperties()
    {
        // Arrange
        var profile = CreateProfile();
        var lunar = CreateLunarContext();

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar
        };

        // Assert
        context.Profile.Should().Be(profile);
        context.Lunar.Should().Be(lunar);
    }

    [Fact]
    public void Profile_Property_IsCorrectlySet()
    {
        // Arrange
        var profile = CreateProfile(Gender.Male, 1990, 2025);

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = CreateLunarContext()
        };

        // Assert
        context.Profile.Gender.Should().Be(Gender.Male);
        context.Profile.LunarYOB.Should().Be(1990);
        context.Profile.TargetYear.Should().Be(2025);
    }

    [Fact]
    public void Lunar_Property_IsCorrectlySet()
    {
        // Arrange
        var lunar = CreateLunarContext(2025, 1, 5, false, 2460635);

        // Act
        var context = new RuleContext
        {
            Profile = CreateProfile(),
            Lunar = lunar
        };

        // Assert
        context.Lunar.LunarYear.Should().Be(2025);
        context.Lunar.LunarMonth.Should().Be(1);
        context.Lunar.LunarDay.Should().Be(5);
        context.Lunar.IsLeap.Should().BeFalse();
        context.Lunar.Jdn.Should().Be(2460635);
    }

    [Fact]
    public void CanChiNgay_CanBeNull()
    {
        // Arrange
        var profile = CreateProfile();
        var lunar = CreateLunarContext();

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar,
            CanChiNgay = null
        };

        // Assert
        context.CanChiNgay.Should().BeNull();
    }

    [Fact]
    public void CanChiTuoi_CanBeNull()
    {
        // Arrange
        var profile = CreateProfile();
        var lunar = CreateLunarContext();

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar,
            CanChiTuoi = null
        };

        // Assert
        context.CanChiTuoi.Should().BeNull();
    }

    [Fact]
    public void BothCanChiFields_CanBeNull()
    {
        // Arrange
        var profile = CreateProfile();
        var lunar = CreateLunarContext();

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar,
            CanChiNgay = null,
            CanChiTuoi = null
        };

        // Assert
        context.CanChiNgay.Should().BeNull();
        context.CanChiTuoi.Should().BeNull();
    }

    [Fact]
    public void CanChiNgay_WhenProvided_IsCorrectlySet()
    {
        // Arrange
        var canChiNgay = new CanChiDay("Giáp", "Tý");

        // Act
        var context = new RuleContext
        {
            Profile = CreateProfile(),
            Lunar = CreateLunarContext(),
            CanChiNgay = canChiNgay
        };

        // Assert
        context.CanChiNgay.Should().NotBeNull();
        context.CanChiNgay!.Can.Should().Be("Giáp");
        context.CanChiNgay.Chi.Should().Be("Tý");
    }

    [Fact]
    public void CanChiTuoi_WhenProvided_IsCorrectlySet()
    {
        // Arrange
        var canChiTuoi = new CanChiYear("Mậu", "Tý");

        // Act
        var context = new RuleContext
        {
            Profile = CreateProfile(),
            Lunar = CreateLunarContext(),
            CanChiTuoi = canChiTuoi
        };

        // Assert
        context.CanChiTuoi.Should().NotBeNull();
        context.CanChiTuoi!.Can.Should().Be("Mậu");
        context.CanChiTuoi.Chi.Should().Be("Tý");
    }

    [Fact]
    public void LeapMonth_IsPreserved()
    {
        // Arrange
        var lunar = CreateLunarContext(2025, 6, 15, true, 2460700);

        // Act
        var context = new RuleContext
        {
            Profile = CreateProfile(),
            Lunar = lunar
        };

        // Assert
        context.Lunar.IsLeap.Should().BeTrue();
    }

    [Fact]
    public void TestFixture_StandardValues_WorkAsExpected()
    {
        // This test validates the standard test fixtures that will be used
        // in other test files.

        // Arrange - Standard fixture from user request
        var profile = new UserProfile
        {
            Gender = Gender.Male,
            LunarYOB = 1990,
            TargetYear = 2025
        };

        var lunar = new LunarDateContext
        {
            LunarYear = 2025,
            LunarMonth = 1,
            LunarDay = 1,
            IsLeap = false,
            Jdn = 2460635
        };

        // Act
        var context = new RuleContext
        {
            Profile = profile,
            Lunar = lunar
        };

        // Assert
        context.Profile.Gender.Should().Be(Gender.Male);
        context.Profile.LunarYOB.Should().Be(1990);
        context.Profile.TargetYear.Should().Be(2025);
        context.Lunar.LunarYear.Should().Be(2025);
        context.Lunar.LunarMonth.Should().Be(1);
        context.Lunar.LunarDay.Should().Be(1);
        context.Lunar.IsLeap.Should().BeFalse();
        context.Lunar.Jdn.Should().Be(2460635);
    }
}
