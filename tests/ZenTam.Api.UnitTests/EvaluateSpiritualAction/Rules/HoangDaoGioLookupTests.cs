using FluentAssertions;
using ZenTam.Api.Features.Calendars.Data;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction.Rules;

public class HoangDaoGioLookupTests
{
    private static readonly string[] ValidChiNames =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    [Fact]
    public void GetHoangGio_All10CanIndices_ReturnsExactly6HoursEach()
    {
        // All 10 Can values (0-9) should return exactly 6 hours
        for (int canIndex = 0; canIndex < 10; canIndex++)
        {
            var hours = HoangDaoGioLookup.GetHoangGio(canIndex);
            hours.Should().HaveCount(6, because: $"Can index {canIndex} should return exactly 6 Hoàng Đạo hours");
        }
    }

    [Fact]
    public void GetHoangGio_All10CanIndices_NoDuplicateHoursWithinCan()
    {
        // For each Can, the 6 hours should be unique (no duplicates)
        for (int canIndex = 0; canIndex < 10; canIndex++)
        {
            var hours = HoangDaoGioLookup.GetHoangGio(canIndex);
            hours.Should().OnlyHaveUniqueItems(because: $"Can index {canIndex} should not have duplicate hours");
        }
    }

    [Fact]
    public void GetHoangGio_All10CanIndices_AllHoursAreValidChiNames()
    {
        // All returned hours should be valid Chi names
        for (int canIndex = 0; canIndex < 10; canIndex++)
        {
            var hours = HoangDaoGioLookup.GetHoangGio(canIndex);
            foreach (var hour in hours)
            {
                ValidChiNames.Should().Contain(hour, because: $"Hour '{hour}' should be a valid Chi name");
            }
        }
    }

    [Fact]
    public void GetHoangGio_All12ChisAppearAcrossAll10Can()
    {
        // Cross-check: all 12 Chis should appear across all 10 Can (some appear multiple times)
        var allHours = new HashSet<string>();
        for (int canIndex = 0; canIndex < 10; canIndex++)
        {
            var hours = HoangDaoGioLookup.GetHoangGio(canIndex);
            foreach (var hour in hours)
            {
                allHours.Add(hour);
            }
        }

        // All 12 Chi names should appear in at least one Can's Hoàng Đạo hours
        foreach (var chi in ValidChiNames)
        {
            allHours.Should().Contain(chi, because: $"Chi '{chi}' should appear in at least one Can's Hoàng Đạo hours");
        }
    }

    [Theory]
    [InlineData(0, "Giáp", new[] { "Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi" })]
    [InlineData(1, "Ất", new[] { "Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất" })]
    [InlineData(2, "Bính", new[] { "Tý", "Sửu", "Thìn", "Tỵ", "Dậu", "Hợi" })]
    [InlineData(3, "Đinh", new[] { "Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất" })]
    [InlineData(4, "Mậu", new[] { "Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi" })]
    [InlineData(5, "Kỷ", new[] { "Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất" })]
    [InlineData(6, "Canh", new[] { "Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất" })]
    [InlineData(7, "Tân", new[] { "Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi" })]
    [InlineData(8, "Nhâm", new[] { "Tý", "Sửu", "Thìn", "Tỵ", "Dậu", "Hợi" })]
    [InlineData(9, "Quý", new[] { "Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất" })]
    public void GetHoangGio_ByCanName_ReturnsCorrectHours(int canIndex, string canName, string[] expectedHours)
    {
        // Act
        var hoursByIndex = HoangDaoGioLookup.GetHoangGio(canIndex);
        var hoursByName = HoangDaoGioLookup.GetHoangGio(canName);

        // Assert
        hoursByIndex.Should().BeEquivalentTo(expectedHours);
        hoursByName.Should().BeEquivalentTo(expectedHours);
    }

    [Fact]
    public void GetHoangGio_InvalidCanName_ThrowsArgumentException()
    {
        // Arrange
        var invalidCanName = "INVALID";

        // Act
        var act = () => HoangDaoGioLookup.GetHoangGio(invalidCanName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Invalid Can name: {invalidCanName}*");
    }

    [Fact]
    public void GetHoangGioDisplay_ReturnsFormattedString()
    {
        // Act
        var display = HoangDaoGioLookup.GetHoangGioDisplay(0);

        // Assert
        display.Should().Contain("Tý");
        display.Should().Contain("Sửu");
        display.Should().Contain("Ngọ");
        display.Should().Contain("Mùi");
        display.Should().Contain("Dậu");
        display.Should().Contain("Hợi");
    }
}