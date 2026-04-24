using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Features.Clients;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.Phase1Data_Clients;

public class ClientCrudTests
{
    private ZenTamDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ZenTamDbContext(options);
    }

    #region H1.x — Create Client Happy Path

    [Fact]
    public async Task H1_1_CreateValidMaleClient_Returns201WithClientData()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new CreateClientHandler(db);
        var request = new CreateClientRequest
        {
            Name = "Nguyễn Văn A",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be("Nguyễn Văn A");
        response.PhoneNumber.Should().Be("0909123456");
        response.Gender.Should().Be(Gender.Male);
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task H1_2_CreateValidFemaleClientWithNotes_Returns201WithNotes()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new CreateClientHandler(db);
        var request = new CreateClientRequest
        {
            Name = "Trần Thị B",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            Notes = "VIP"
        };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Notes.Should().Be("VIP");
        response.Gender.Should().Be(Gender.Female);
    }

    [Fact]
    public async Task H1_3_MinimumValidClient_Returns201()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new CreateClientHandler(db);
        var request = new CreateClientRequest
        {
            Name = "Lê Văn C",
            PhoneNumber = "0909123458",
            SolarDob = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Id.Should().NotBeEmpty();
    }

    #endregion

    #region E1.x — Create Client Validation Errors

    [Fact]
    public async Task E1_1_MissingName_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task E1_2_NameExceeds100Chars_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = new string('A', 101),
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task E1_3_MissingPhoneNumber_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task E1_4_PhoneNumberTooShort_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "090",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task E1_5_PhoneNumberWithLetters_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "abcdefghij",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task E1_6_MissingSolarDob_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SolarDob");
    }

    [Fact]
    public async Task E1_7_FutureSolarDob_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = DateTime.UtcNow.AddDays(1),
            Gender = Gender.Male
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SolarDob");
    }

    [Fact]
    public async Task E1_8_MissingGender_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Gender");
    }

    [Fact]
    public async Task E1_9_GenderOutOfRange_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = (Gender)2
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Gender");
    }

    [Fact]
    public async Task E1_10_GenderNegative_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = (Gender)(-1)
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Gender");
    }

    [Fact]
    public async Task E1_11_NotesExceeds1000Chars_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateClientValidator();
        var request = new CreateClientRequest
        {
            Name = "Test",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            Notes = new string('x', 1001)
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }

    #endregion

    #region H2.x — Search Clients by Phone

    [Fact]
    public async Task H2_1_ExactPhoneMatch_ReturnsSingleClient()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        db.ClientProfiles.Add(client);
        await db.SaveChangesAsync();

        var handler = new SearchClientsHandler(db);
        var request = new SearchClientsRequest { Phone = "0909123456" };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Clients.Should().HaveCount(1);
        response.Clients[0].PhoneNumber.Should().Be("0909123456");
    }

    [Fact]
    public async Task H2_2_PrefixMatch_ReturnsMatchingClients()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Client 1",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Client 2",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1999, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new SearchClientsHandler(db);
        var request = new SearchClientsRequest { Phone = "0909" };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Clients.Should().HaveCount(2);
    }

    [Fact]
    public async Task H2_3_NoMatch_ReturnsEmptyArray()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new SearchClientsHandler(db);
        var request = new SearchClientsRequest { Phone = "0999" };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Clients.Should().BeEmpty();
    }

    [Fact]
    public async Task H2_4_PartialMidStringMatch_ReturnsMatchingClients()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = "Client 1",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new SearchClientsHandler(db);
        var request = new SearchClientsRequest { Phone = "123" };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Clients.Should().HaveCount(1);
    }

    #endregion

    #region H3.x — Get Client by ID

    [Fact]
    public async Task H3_1_ClientExistsNoRelatedPersons_ReturnsClientWithEmptyRelatedPersons()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var clientId = Guid.NewGuid();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetClientHandler(db);
        var request = new GetClientRequest { Id = clientId };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response!.RelatedPersons.Should().BeEmpty();
    }

    [Fact]
    public async Task H3_2_ClientWithRelatedPersons_ReturnsClientWithTwoRelatedPersons()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var clientId = Guid.NewGuid();
        var client = new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        client.RelatedPersons.Add(new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Label = "Vợ",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });
        client.RelatedPersons.Add(new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Label = "Con Trai",
            SolarDob = new DateTime(2020, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        db.ClientProfiles.Add(client);
        await db.SaveChangesAsync();

        var handler = new GetClientHandler(db);
        var request = new GetClientRequest { Id = clientId };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response!.RelatedPersons.Should().HaveCount(2);
    }

    [Fact]
    public async Task E3_1_ClientNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new GetClientHandler(db);
        var request = new GetClientRequest { Id = Guid.NewGuid() };

        // Act & Assert
        var act = async () => await handler.HandleAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region H4.x — Add Related Person

    [Fact]
    public async Task H4_1_AddSpouse_Returns201WithNewRelatedPerson()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var clientId = Guid.NewGuid();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new AddRelatedPersonHandler(db);
        var request = new AddRelatedPersonRequest
        {
            ClientId = clientId,
            Label = "Vợ",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female
        };

        // Act
        var response = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ClientId.Should().Be(clientId);
        response.Label.Should().Be("Vợ");
    }

    [Fact]
    public async Task E4_7_ParentClientNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new AddRelatedPersonHandler(db);
        var request = new AddRelatedPersonRequest
        {
            ClientId = Guid.NewGuid(),
            Label = "Vợ",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female
        };

        // Act & Assert
        var act = async () => await handler.HandleAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region H5.x — Delete Related Person

    [Fact]
    public async Task H5_1_ValidDelete_Returns204NoContent()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var clientId = Guid.NewGuid();
        var relatedId = Guid.NewGuid();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        db.ClientRelatedPersons.Add(new ClientRelatedPerson
        {
            Id = relatedId,
            ClientId = clientId,
            Label = "Vợ",
            SolarDob = new DateTime(1999, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteRelatedPersonHandler(db);

        // Act
        await handler.HandleAsync(clientId, relatedId, CancellationToken.None);

        // Assert - verify deletion
        var deleted = await db.ClientRelatedPersons.FindAsync(relatedId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task E5_1_ClientNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var handler = new DeleteRelatedPersonHandler(db);

        // Act & Assert
        var act = async () => await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task E5_2_RelatedPersonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var clientId = Guid.NewGuid();
        db.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteRelatedPersonHandler(db);

        // Act & Assert
        var act = async () => await handler.HandleAsync(clientId, Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
