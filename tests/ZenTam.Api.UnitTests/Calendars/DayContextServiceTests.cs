using Moq;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;

namespace ZenTam.Api.UnitTests.Calendars;

public class DayContextServiceTests
{
    private readonly Mock<ICanChiCalculator> _mockCanChiCalculator;
    private readonly Mock<ILunarCalculatorService> _mockLunarCalculator;
    private readonly Mock<ISolarTermCalculator> _mockSolarTermCalculator;
    private readonly DayContextService _service;

    public DayContextServiceTests()
    {
        _mockCanChiCalculator = new Mock<ICanChiCalculator>();
        _mockLunarCalculator = new Mock<ILunarCalculatorService>();
        _mockSolarTermCalculator = new Mock<ISolarTermCalculator>();
        _service = new DayContextService(
            _mockCanChiCalculator.Object,
            _mockLunarCalculator.Object,
            _mockSolarTermCalculator.Object
        );
    }

    [Fact]
    public void GetDayContext_2026_05_12_ReturnsFullContext()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        int jdn = 2460847; // JDN for 2026-05-12

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 5, 12)).Returns(jdn);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 3, LunarDay = 16, IsLeap = false });
        
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Bính", "Ngọ"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiThang(2026, 3, false))
            .Returns(new CanChiMonth("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetThapNhiTruc(solarDate)).Returns(5);
        _mockCanChiCalculator.Setup(x => x.GetTrucName(5)).Returns("Bình");
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(22);

        // Act
        var result = _service.GetDayContext(solarDate);

        // Assert
        Assert.Equal(solarDate, result.SolarDate);
        Assert.Equal("Bính Ngọ", result.CanChiNgay);
        Assert.Equal("Bính Dần", result.CanChiThang);
        Assert.Equal(5, result.TrucIndex);
        Assert.Equal("Bình", result.TrucName);
        Assert.NotNull(result.NhiThapBatTu);
        Assert.NotNull(result.HoangDao);
        Assert.NotNull(result.SatChu);
        Assert.NotNull(result.ThuTu);
    }

    [Fact]
    public void GetDayContext_AllFields_NonNull()
    {
        // Arrange
        var solarDate = new DateTime(2026, 1, 1);
        int jdn = 2460592;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 1, 1)).Returns(jdn);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 1, LunarDay = 1, IsLeap = false });
        
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Giáp", "Tý"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiThang(2026, 1, false))
            .Returns(new CanChiMonth("Giáp", "Tý"));
        _mockCanChiCalculator.Setup(x => x.GetThapNhiTruc(solarDate)).Returns(0);
        _mockCanChiCalculator.Setup(x => x.GetTrucName(0)).Returns("Kiến");
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(0);

        // Act
        var result = _service.GetDayContext(solarDate);

        // Assert - all fields populated
        Assert.NotNull(result.NhiThapBatTu);
        Assert.NotNull(result.HoangDao);
        Assert.NotNull(result.SatChu);
        Assert.NotNull(result.ThuTu);
        Assert.NotNull(result.CanChiNgay);
        Assert.NotNull(result.CanChiThang);
        Assert.NotNull(result.TrucName);
    }

    [Fact]
    public void GetHoangDao_2026_05_12_BinhNgo_ReturnsPatternA()
    {
        // Arrange - Bính Ngọ matches Pattern A (canIndex=2, chiIndex=6)
        var solarDate = new DateTime(2026, 5, 12);
        int jdn = 2460847;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 5, 12)).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Bính", "Ngọ"));

        // Act
        var result = _service.GetHoangDao(solarDate);

        // Assert
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Contains("Tý", result.HoangDaoHours);
        Assert.Contains("Sửu", result.HoangDaoHours);
        Assert.Contains("Ngọ", result.HoangDaoHours);
        Assert.Contains("Mùi", result.HoangDaoHours);
        Assert.Contains("Dậu", result.HoangDaoHours);
        Assert.Contains("Hợi", result.HoangDaoHours);
    }

    [Fact]
    public void GetNhiThapBatTu_Index0_ReturnsKiettu()
    {
        // Arrange
        var solarDate = new DateTime(2026, 1, 1);
        int jdn = 2460592;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 1, 1)).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(0);

        // Act
        var result = _service.GetNhiThapBatTu(solarDate);

        // Assert
        Assert.Equal(0, result.Index);
        Assert.Equal("Côn", result.Name);
        Assert.Equal(TuClassification.Kiettu, result.Classification);
    }

    [Fact]
    public void GetNhiThapBatTu_Index12_ReturnsHungtu()
    {
        // Arrange
        var solarDate = new DateTime(2026, 3, 15);
        int jdn = 2460645;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 3, 15)).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(12);

        // Act
        var result = _service.GetNhiThapBatTu(solarDate);

        // Assert
        Assert.Equal(12, result.Index);
        Assert.Equal("Mão", result.Name);
        Assert.Equal(TuClassification.Hungtu, result.Classification);
    }

    [Fact]
    public void GetNhiThapBatTu_Index23_ReturnsBinhtu()
    {
        // Arrange
        var solarDate = new DateTime(2026, 6, 20);
        int jdn = 2460742;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 6, 20)).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(23);

        // Act
        var result = _service.GetNhiThapBatTu(solarDate);

        // Assert
        Assert.Equal(23, result.Index);
        Assert.Equal("Quân", result.Name);
        Assert.Equal(TuClassification.Binhtu, result.Classification);
    }

    [Theory]
    [InlineData(0, TuClassification.Kiettu)]
    [InlineData(1, TuClassification.Kiettu)]
    [InlineData(2, TuClassification.Kiettu)]
    [InlineData(3, TuClassification.Kiettu)]
    [InlineData(4, TuClassification.Kiettu)]
    [InlineData(5, TuClassification.Kiettu)]
    public void GetNhiThapBatTu_Indices0to5_ReturnsKiettu(int index, TuClassification expected)
    {
        var solarDate = new DateTime(2026, 1, 1);
        int jdn = 2460592;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(index);

        var result = _service.GetNhiThapBatTu(solarDate);
        Assert.Equal(expected, result.Classification);
    }

    [Theory]
    [InlineData(12, TuClassification.Hungtu)]
    [InlineData(13, TuClassification.Hungtu)]
    [InlineData(14, TuClassification.Hungtu)]
    [InlineData(15, TuClassification.Hungtu)]
    [InlineData(16, TuClassification.Hungtu)]
    [InlineData(17, TuClassification.Hungtu)]
    [InlineData(18, TuClassification.Hungtu)]
    [InlineData(19, TuClassification.Hungtu)]
    [InlineData(20, TuClassification.Hungtu)]
    [InlineData(21, TuClassification.Hungtu)]
    [InlineData(22, TuClassification.Hungtu)]
    public void GetNhiThapBatTu_Indices12to22_ReturnsHungtu(int index, TuClassification expected)
    {
        var solarDate = new DateTime(2026, 1, 1);
        int jdn = 2460592;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(index);

        var result = _service.GetNhiThapBatTu(solarDate);
        Assert.Equal(expected, result.Classification);
    }

    [Fact]
    public void GetSatChu_MatchingDay_ReturnsTrue()
    {
        // Arrange - lunar month 1, day 9 should be Sát Chủ
        var solarDate = new DateTime(2026, 2, 17);
        
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 1, LunarDay = 9, IsLeap = false });
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(2460592);

        // Act
        var result = _service.GetSatChu(solarDate);

        // Assert
        Assert.True(result.IsSatChu);
        Assert.Equal(9, result.LunarDay);
    }

    [Fact]
    public void GetSatChu_NonMatchingDay_ReturnsFalse()
    {
        // Arrange - lunar month 1, day 10 should NOT be Sát Chủ
        var solarDate = new DateTime(2026, 2, 18);
        
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 1, LunarDay = 10, IsLeap = false });
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(2460593);

        // Act
        var result = _service.GetSatChu(solarDate);

        // Assert
        Assert.False(result.IsSatChu);
    }

    [Fact]
    public void GetThuTu_MatchingChi_ReturnsTrue()
    {
        // Arrange - lunar month 1, chi Tý (index 0) or Ngọ (index 6) should be Thọ Tử
        var solarDate = new DateTime(2026, 2, 15);
        int jdn = 2460646;
        
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 1, LunarDay = 15, IsLeap = false });
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Giáp", "Tý"));

        // Act
        var result = _service.GetThuTu(solarDate);

        // Assert
        Assert.True(result.IsThuTu);
        Assert.Equal(2, result.ForbiddenChi.Length);
    }

    [Fact]
    public void GetThuTu_NonMatchingChi_ReturnsFalse()
    {
        // Arrange - lunar month 1, chi Sửu (index 1) should NOT be Thọ Tử
        var solarDate = new DateTime(2026, 2, 16);
        int jdn = 2460647;
        
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 1, LunarDay = 16, IsLeap = false });
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(jdn);
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Ất", "Sửu"));

        // Act
        var result = _service.GetThuTu(solarDate);

        // Assert
        Assert.False(result.IsThuTu);
        Assert.Empty(result.ForbiddenChi);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(14)]
    [InlineData(23)]
    public void GetDayContext_IsNgayKy_TrueForSpecialDays(int lunarDay)
    {
        // Arrange
        var solarDate = new DateTime(2026, 3, 1);
        int jdn = 2460645;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 3, 1)).Returns(jdn);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 3, LunarDay = lunarDay, IsLeap = false });
        
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiThang(2026, 3, false))
            .Returns(new CanChiMonth("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetThapNhiTruc(solarDate)).Returns(0);
        _mockCanChiCalculator.Setup(x => x.GetTrucName(0)).Returns("Kiến");
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(0);

        // Act
        var result = _service.GetDayContext(solarDate);

        // Assert
        Assert.True(result.IsNgayKy);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    public void GetDayContext_IsNgayKy_FalseForOtherDays(int lunarDay)
    {
        // Arrange
        var solarDate = new DateTime(2026, 3, 1);
        int jdn = 2460645;

        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 3, 1)).Returns(jdn);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 3, LunarDay = lunarDay, IsLeap = false });
        
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(jdn))
            .Returns(new CanChiDay("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiThang(2026, 3, false))
            .Returns(new CanChiMonth("Bính", "Dần"));
        _mockCanChiCalculator.Setup(x => x.GetThapNhiTruc(solarDate)).Returns(0);
        _mockCanChiCalculator.Setup(x => x.GetTrucName(0)).Returns("Kiến");
        _mockCanChiCalculator.Setup(x => x.GetNhiThapBatTu(jdn)).Returns(0);

        // Act
        var result = _service.GetDayContext(solarDate);

        // Assert
        Assert.False(result.IsNgayKy);
    }
}
