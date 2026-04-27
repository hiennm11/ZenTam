using Moq;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;

namespace ZenTam.Api.UnitTests.Calendars;

public class HoangDaoServiceTests
{
    private readonly Mock<IDayContextService> _mockDayContextService;
    private readonly HoangDaoService _service;

    public HoangDaoServiceTests()
    {
        _mockDayContextService = new Mock<IDayContextService>();
        _service = new HoangDaoService(_mockDayContextService.Object);
    }

    [Fact]
    public void GetHoangDao_CallsDayContextService()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedInfo = new HoangDaoInfo(
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockDayContextService.Setup(x => x.GetHoangDao(solarDate)).Returns(expectedInfo);

        // Act
        var result = _service.GetHoangDao(solarDate);

        // Assert
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        _mockDayContextService.Verify(x => x.GetHoangDao(solarDate), Times.Once);
    }

    [Fact]
    public void GetHoangDaoResponse_ReturnsCorrectStructure()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var hoangDaoInfo = new HoangDaoInfo(
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: 5,
            TrucName: "Bình",
            NhiThapBatTu: new NhiThapBatTuInfo(22, "Dương", TuClassification.Hungtu),
            HoangDao: hoangDaoInfo,
            SatChu: new SatChuInfo(false, 9),
            ThuTu: new ThuTuInfo(false, []),
            IsNgayKy: false
        );

        _mockDayContextService.Setup(x => x.GetHoangDao(solarDate)).Returns(hoangDaoInfo);
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);

        // Act
        var result = _service.GetHoangDaoResponse(solarDate);

        // Assert
        Assert.Equal(solarDate, result.SolarDate);
        Assert.Equal("Bính Ngọ", result.CanChiNgay);
        Assert.True(result.IsHoangDao);
        Assert.Equal(6, result.HoangDaoHours.Count);
        Assert.Equal(6, result.HacDaoHours.Count);
        Assert.Equal(3, result.TopHours.Count);
    }

    [Fact]
    public void GetHoangDaoResponse_TopHours_ReturnsTop3FromHoangDao()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var hoangDaoInfo = new HoangDaoInfo(
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: 5,
            TrucName: "Bình",
            NhiThapBatTu: new NhiThapBatTuInfo(22, "Dương", TuClassification.Hungtu),
            HoangDao: hoangDaoInfo,
            SatChu: new SatChuInfo(false, 9),
            ThuTu: new ThuTuInfo(false, []),
            IsNgayKy: false
        );

        _mockDayContextService.Setup(x => x.GetHoangDao(solarDate)).Returns(hoangDaoInfo);
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);

        // Act
        var result = _service.GetHoangDaoResponse(solarDate);

        // Assert
        Assert.Equal(3, result.TopHours.Count);
        Assert.Contains("Tý", result.TopHours);
        Assert.Contains("Ngọ", result.TopHours);
        Assert.Contains("Mùi", result.TopHours);
    }

    [Fact]
    public void GetHoangDaoResponse_NotHoangDao_ReturnsEmptyLists()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var hoangDaoInfo = new HoangDaoInfo(
            IsHoangDao: false,
            HoangDaoHours: [],
            HacDaoHours: ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"],
            TopHours: []
        );
        var dayContext = new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Giáp Sửu",
            CanChiThang: "Bính Dần",
            TrucIndex: 5,
            TrucName: "Bình",
            NhiThapBatTu: new NhiThapBatTuInfo(22, "Dương", TuClassification.Hungtu),
            HoangDao: hoangDaoInfo,
            SatChu: new SatChuInfo(false, 9),
            ThuTu: new ThuTuInfo(false, []),
            IsNgayKy: false
        );

        _mockDayContextService.Setup(x => x.GetHoangDao(solarDate)).Returns(hoangDaoInfo);
        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);

        // Act
        var result = _service.GetHoangDaoResponse(solarDate);

        // Assert
        Assert.False(result.IsHoangDao);
        Assert.Empty(result.HoangDaoHours);
        Assert.Empty(result.TopHours);
        Assert.Equal(12, result.HacDaoHours.Count);
    }
}
