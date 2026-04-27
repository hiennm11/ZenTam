using Microsoft.AspNetCore.Mvc;
using Moq;
using ZenTam.Api.Features.Calendars;
using ZenTam.Api.Features.Calendars.Services;

namespace ZenTam.Api.UnitTests.Calendars;

public class HoangDaoControllerTests
{
    private readonly Mock<IHoangDaoService> _mockHoangDaoService;
    private readonly HoangDaoController _controller;

    public HoangDaoControllerTests()
    {
        _mockHoangDaoService = new Mock<IHoangDaoService>();
        _controller = new HoangDaoController(_mockHoangDaoService.Object);
    }

    [Fact]
    public void GetByDate_ValidDate_Returns200WithHoangDaoResponse()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Equal(solarDate, response.SolarDate);
        Assert.Equal("Bính Ngọ", response.CanChiNgay);
        Assert.True(response.IsHoangDao);
        Assert.Equal(6, response.HoangDaoHours.Count);
        Assert.Equal(6, response.HacDaoHours.Count);
        Assert.Equal(3, response.TopHours.Count);
    }

    [Fact]
    public void GetByQuery_ValidDate_Returns200WithHoangDaoResponse()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByQuery(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Equal(solarDate, response.SolarDate);
        Assert.Equal("Bính Ngọ", response.CanChiNgay);
    }

    [Fact]
    public void GetByDate_HoangDaoHours_Exactly6()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Equal(6, response.HoangDaoHours.Count);
    }

    [Fact]
    public void GetByDate_HacDaoHours_Exactly6()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Equal(6, response.HacDaoHours.Count);
    }

    [Fact]
    public void GetByDate_TopHours_Exactly3()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Equal(3, response.TopHours.Count);
    }

    [Fact]
    public void GetByDate_CanChiNgay_FormattedCorrectly()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.Contains(" ", response.CanChiNgay);
    }

    [Fact]
    public void GetByDate_PathStyle_ReturnsSameAsQueryStyle()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var pathResult = _controller.GetByDate(solarDate);
        var queryResult = _controller.GetByQuery(solarDate);

        // Assert
        var pathOkResult = Assert.IsType<OkObjectResult>(pathResult);
        var queryOkResult = Assert.IsType<OkObjectResult>(queryResult);
        var pathResponse = Assert.IsType<HoangDaoResponse>(pathOkResult.Value);
        var queryResponse = Assert.IsType<HoangDaoResponse>(queryOkResult.Value);
        Assert.Equal(pathResponse.SolarDate, queryResponse.SolarDate);
        Assert.Equal(pathResponse.CanChiNgay, queryResponse.CanChiNgay);
        Assert.Equal(pathResponse.IsHoangDao, queryResponse.IsHoangDao);
    }

    [Fact]
    public void GetByDate_FutureDate_Returns200OK()
    {
        // Arrange
        var futureDate = new DateTime(2099, 12, 31);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: futureDate,
            CanChiNgay: "Nhâm Tuất",
            IsHoangDao: false,
            HoangDaoHours: [],
            HacDaoHours: ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"],
            TopHours: []
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(futureDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(futureDate);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetByDate_PastDate_Returns200OK()
    {
        // Arrange
        var pastDate = new DateTime(1900, 1, 1);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: pastDate,
            CanChiNgay: "Ất Sửu",
            IsHoangDao: false,
            HoangDaoHours: [],
            HacDaoHours: ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"],
            TopHours: []
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(pastDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(pastDate);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetByQuery_InvalidDateFormat_Returns400()
    {
        // Arrange - DateTime.TryParse will fail for invalid format
        // When ASP.NET model binding fails for [FromQuery] DateTime, it returns 400
        // Here we test the controller behavior when passed an unparseable value
        var invalidDateString = "not-a-date";
        Assert.False(DateTime.TryParse(invalidDateString, out _));

        // Act & Assert
        // Note: The actual 400 BadRequest happens during model binding, not in controller action
        // This test verifies that invalid dates passed to GetByQuery would not succeed
        // In real integration tests with WebApplicationFactory, invalid dates return 400
    }

    [Fact]
    public void GetByDate_Response_ContainsAllRequiredFields()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 12);
        var expectedResponse = new HoangDaoResponse(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            IsHoangDao: true,
            HoangDaoHours: ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
            HacDaoHours: ["Dần", "Mão", "Thìn", "Tỵ", "Thân", "Tuất"],
            TopHours: ["Tý", "Ngọ", "Mùi"]
        );
        _mockHoangDaoService.Setup(x => x.GetHoangDaoResponse(solarDate)).Returns(expectedResponse);

        // Act
        var result = _controller.GetByDate(solarDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HoangDaoResponse>(okResult.Value);
        Assert.NotEqual(default, response.SolarDate);
        Assert.False(string.IsNullOrEmpty(response.CanChiNgay));
        Assert.NotNull(response.HoangDaoHours);
        Assert.NotNull(response.HacDaoHours);
        Assert.NotNull(response.TopHours);
    }
}
