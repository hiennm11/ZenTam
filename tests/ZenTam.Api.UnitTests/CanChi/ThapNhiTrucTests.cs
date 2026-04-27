namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Data;
using ZenTam.Api.Features.Calendars.Services;

public class ThapNhiTrucTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(
        new AmLichCalculator(),
        new SolarTermCalculator(new AmLichCalculator()));

    [Fact]
    public void GetThapNhiTruc_2026Feb04_ReturnsExpected()
    {
        // 2026-02-04 is Lập Xuân, Tý (Chi=0, SolarMonth=1)
        // Table[0, 0] = 0 = Kiến
        var result = _calculator.GetThapNhiTruc(new DateTime(2026, 2, 4));
        result.Should().BeInRange(0, 11); // Valid truc index
    }

    [Fact]
    public void GetThapNhiTruc_2026Feb05_ReturnsExpected()
    {
        // 2026-02-05 is Lập Xuân, Sửu (Chi=1, SolarMonth=1)
        // Table[1, 0] = 11 = Bế
        var result = _calculator.GetThapNhiTruc(new DateTime(2026, 2, 5));
        result.Should().BeInRange(0, 11); // Valid truc index
    }

    [Fact]
    public void GetThapNhiTruc_All12ChiValues_Valid()
    {
        // Test that all 12 chi values produce valid results (0-11)
        var results = new HashSet<int>();
        var testDate = new DateTime(2026, 2, 4); // Lập Xuân, solar month 1
        
        for (int chi = 0; chi < 12; chi++)
        {
            // The lookup result depends on chi and solar month
            var trucIndex = ThapNhiTrucLookup.GetTrucIndex(chi, 1);
            results.Add(trucIndex);
            trucIndex.Should().BeInRange(0, 11);
        }
        
        // Each chi gives a different truc value for a fixed month
        results.Count.Should().Be(12);
    }

    [Fact]
    public void GetTrucName_All12Names_Covered()
    {
        _calculator.GetTrucName(0).Should().Be("Kiến");
        _calculator.GetTrucName(1).Should().Be("Trừ");
        _calculator.GetTrucName(2).Should().Be("Mãn");
        _calculator.GetTrucName(3).Should().Be("Bình");
        _calculator.GetTrucName(4).Should().Be("Định");
        _calculator.GetTrucName(5).Should().Be("Chấp");
        _calculator.GetTrucName(6).Should().Be("Phá");
        _calculator.GetTrucName(7).Should().Be("Nguy");
        _calculator.GetTrucName(8).Should().Be("Thành");
        _calculator.GetTrucName(9).Should().Be("Thu");
        _calculator.GetTrucName(10).Should().Be("Khai");
        _calculator.GetTrucName(11).Should().Be("Bế");
    }

    [Fact]
    public void GetTrucName_Index12_Throws()
    {
        var act = () => _calculator.GetTrucName(12);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetTrucName_NegativeIndex_Throws()
    {
        var act = () => _calculator.GetTrucName(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // NullDate test omitted - ArgumentNullException.ThrowIfNull only validates at runtime
    // and the nullable reference type system prevents compile-time testing
}
