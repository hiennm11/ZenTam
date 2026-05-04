using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.IntegrationTests.Fixtures;

namespace ZenTam.Api.IntegrationTests.Clients;

public class CreateClientEndpointTests : IClassFixture<Fixtures.ZenTamApiFactory>
{
    private readonly ZenTamApiFactory _factory;
    private readonly HttpClient _client;

    public CreateClientEndpointTests(Fixtures.ZenTamApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task H1_1_CreateValidMaleClient_Returns201WithClientData()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Nguyễn Văn A",
            Username = $"user_{Guid.NewGuid():N}",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateClientResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Nguyễn Văn A");
        result.PhoneNumber.Should().Be("0909123456");
    }

    [Fact]
    public async Task H1_2_CreateValidFemaleClientWithNotes_Returns201WithNotes()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Trần Thị B",
            Username = $"user_{Guid.NewGuid():N}",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            Notes = "VIP"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateClientResponse>();
        result.Should().NotBeNull();
        result!.Notes.Should().Be("VIP");
    }

    [Fact]
    public async Task H1_3_MinimumValidClient_Returns201WithGeneratedId()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Lê Văn C",
            Username = $"user_{Guid.NewGuid():N}",
            PhoneNumber = "0909123458",
            SolarDob = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateClientResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task E1_1_MissingName_Returns400WithNameError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task E1_2_NameExceeds100Chars_Returns400WithNameError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = new string('A', 101),
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task E1_3_MissingPhoneNumber_Returns400WithPhoneNumberError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("PhoneNumber");
    }

    [Fact]
    public async Task E1_4_PhoneNumberTooShort_Returns400WithPhoneNumberError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "090",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("PhoneNumber");
    }

    [Fact]
    public async Task E1_5_PhoneNumberWithLetters_Returns400WithPhoneNumberError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "abcdefghij",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("PhoneNumber");
    }

    [Fact]
    public async Task E1_6_MissingSolarDob_Returns400WithSolarDobError()
    {
        // Arrange - Use anonymous type with only required fields to simulate missing SolarDob
        var request = new
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            Gender = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("SolarDob");
    }

    [Fact]
    public async Task E1_7_FutureSolarDob_Returns400WithSolarDobError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(2099, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("SolarDob");
    }

    [Fact]
    public async Task E1_8_MissingGender_Returns400WithGenderError()
    {
        // Arrange - Use anonymous type without Gender to simulate missing field
        var request = new
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Gender");
    }

    [Fact]
    public async Task E1_9_NotesExceeds1000Chars_Returns400WithNotesError()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            Notes = new string('x', 1001)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Notes");
    }
}