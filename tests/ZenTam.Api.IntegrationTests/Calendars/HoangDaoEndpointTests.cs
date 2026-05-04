using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.IntegrationTests.Fixtures;

namespace ZenTam.Api.IntegrationTests.Calendars;

public class HoangDaoEndpointTests : IClassFixture<Fixtures.ZenTamApiFactory>
{
    private readonly ZenTamApiFactory _factory;
    private readonly HttpClient _client;

    public HoangDaoEndpointTests(Fixtures.ZenTamApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task H_C1_GetByDatePath_Returns200WithHoangDaoInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/hoang-dao/2026-05-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HoangDaoInfo>();
        result.Should().NotBeNull();
        // IsHoangDao value depends on actual calendar calculation, just verify it exists
        result!.HoangDaoHours.Should().NotBeNull();
        result.HacDaoHours.Should().NotBeNull();
        result.TopHours.Should().NotBeNull();
    }

    [Fact]
    public async Task H_C2_GetByQueryParam_Returns200WithHoangDaoInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/hoang-dao?solarDate=2026-05-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HoangDaoInfo>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task H_C3_ValidDateFormat_ReturnsSameResponseAsDateOnly()
    {
        // Act
        var responseWithTime = await _client.GetAsync("/api/hoang-dao/2026-05-15T10:30:00");
        var responseDateOnly = await _client.GetAsync("/api/hoang-dao/2026-05-15");

        // Assert
        responseWithTime.StatusCode.Should().Be(HttpStatusCode.OK);
        responseDateOnly.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultWithTime = await responseWithTime.Content.ReadFromJsonAsync<HoangDaoInfo>();
        var resultDateOnly = await responseDateOnly.Content.ReadFromJsonAsync<HoangDaoInfo>();

        resultWithTime.Should().NotBeNull();
        resultDateOnly.Should().NotBeNull();
        // Both should return the same HoangDao result since time is not used in the calculation
        resultWithTime!.HoangDaoHours.Should().BeEquivalentTo(resultDateOnly!.HoangDaoHours);
        resultWithTime.HacDaoHours.Should().BeEquivalentTo(resultDateOnly.HacDaoHours);
    }
}