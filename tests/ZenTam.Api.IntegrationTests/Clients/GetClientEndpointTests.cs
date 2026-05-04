using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.Features.Clients.Queries;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.IntegrationTests.Fixtures;

namespace ZenTam.Api.IntegrationTests.Clients;

public class GetClientEndpointTests : IClassFixture<Fixtures.ZenTamApiFactory>
{
    private readonly ZenTamApiFactory _factory;
    private readonly HttpClient _client;

    public GetClientEndpointTests(Fixtures.ZenTamApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task H3_1_ClientExistsNoRelatedPersons_Returns200WithEmptyRelatedPersons()
    {
        // Arrange - Create a client first
        var createRequest = new CreateClientRequest
        {
            Name = "Nguyễn Văn A",
            Username = $"user_{Guid.NewGuid():N}",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        var createResponse = await _client.PostAsJsonAsync("/api/clients", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>();
        var clientId = created!.Id;

        // Act
        var response = await _client.GetAsync($"/api/clients/{clientId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetClientResponse>();
        result.Should().NotBeNull();
        result!.RelatedPersons.Should().BeEmpty();
    }

    [Fact]
    public async Task H3_2_ClientWithRelatedPersons_Returns200WithTwoRelatedPersons()
    {
        // Arrange - Create client and add related persons
        var createRequest = new CreateClientRequest
        {
            Name = "Trần Thị B",
            Username = $"user_{Guid.NewGuid():N}",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female
        };

        var createResponse = await _client.PostAsJsonAsync("/api/clients", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>();
        var clientId = created!.Id;

        // Add first related person (spouse)
        var addRelated1 = new AddRelatedPersonRequest
        {
            ClientId = clientId,
            Label = "Vợ",
            SolarDob = new DateTime(2000, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female
        };
        await _client.PostAsJsonAsync($"/api/clients/{clientId}/related", addRelated1);

        // Add second related person (child)
        var addRelated2 = new AddRelatedPersonRequest
        {
            ClientId = clientId,
            Label = "Con",
            SolarDob = new DateTime(2020, 7, 5, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };
        await _client.PostAsJsonAsync($"/api/clients/{clientId}/related", addRelated2);

        // Act
        var response = await _client.GetAsync($"/api/clients/{clientId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetClientResponse>();
        result.Should().NotBeNull();
        result!.RelatedPersons.Should().HaveCount(2);
    }

    [Fact]
    public async Task E3_1_ClientNotFound_Returns404ProblemDetail()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/clients/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
    }
}