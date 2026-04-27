namespace ZenTam.Api.UnitTests.CanChi;

using FluentAssertions;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Services;

public class NhiThapBatTuTests
{
    private readonly ICanChiCalculator _calculator = new CanChiCalculator(
        new AmLichCalculator(),
        new SolarTermCalculator(new AmLichCalculator()));

    [Fact]
    public void GetNhiThapBatTu_All_28_Values_Covered()
    {
        // Test that all 28 values are covered in a 28-day cycle
        var jdn = 2444235; // 1984-01-01
        var results = new HashSet<int>();
        
        for (int i = 0; i < 28; i++)
        {
            results.Add(_calculator.GetNhiThapBatTu(jdn + i));
        }
        
        results.Count.Should().Be(28);
    }

    [Fact]
    public void GetNhiThapBatTu_Cycle_Repeats_After_28_Days()
    {
        var jdn = 5000000;
        var resultAtJdn = _calculator.GetNhiThapBatTu(jdn);
        var resultAtJdnPlus28 = _calculator.GetNhiThapBatTu(jdn + 28);
        
        resultAtJdn.Should().Be(resultAtJdnPlus28);
    }

    [Fact]
    public void GetNhiThapBatTu_Value_InRange()
    {
        var jdn = 2444235;
        for (int i = 0; i < 100; i++)
        {
            var result = _calculator.GetNhiThapBatTu(jdn + i);
            result.Should().BeInRange(0, 27);
        }
    }
}
