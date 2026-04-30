using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Models;
using ZenTam.Api.Domain.Services;
using ZenTam.Api.Features.EvaluateSpiritualAction.Queries;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.Phase1_EvaluateSpiritualAction;

public class EvaluateSpiritualActionTests
{
    private readonly ZenTamDbContext _db;
    private readonly RuleResolver _ruleResolver;
    private readonly Mock<ILunarCalculatorService> _lunarCalculatorMock;
    private readonly Mock<IGanhMenhService> _ganhMenhServiceMock;
    private readonly Mock<ICanChiCalculator> _canChiCalculatorMock;
    private readonly EvaluateActionHandler _handler;

    // Test data IDs from contract spec (Section 4)
    private static readonly Guid SeedClientId = new("3c7be808-02c1-4f24-85e1-26f0f2455675");
    private static readonly Guid Male1990Id = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Female1996Id = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Male1986Id = new("33333333-3333-3333-3333-333333333333");

    public EvaluateSpiritualActionTests()
    {
        _db = CreateInMemoryDbContext();
        var rules = new ISpiritualRule[]
        {
            new KimLauRule(),
            new HoangOcRule(),
            new TamTaiRule(),
            new ThaiTueRule()
        };
        _ruleResolver = new RuleResolver(rules);
        _lunarCalculatorMock = new Mock<ILunarCalculatorService>();
        _ganhMenhServiceMock = new Mock<IGanhMenhService>();
        _canChiCalculatorMock = new Mock<ICanChiCalculator>();
        _canChiCalculatorMock.Setup(c => c.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Giáp", "Tý"));
        _canChiCalculatorMock.Setup(c => c.GetJulianDayNumber(It.IsAny<DateTime>()))
            .Returns(2460000);
        _canChiCalculatorMock.Setup(c => c.GetCanChiNgay(It.IsAny<int>()))
            .Returns(new CanChiDay("Giáp", "Tý"));
        _handler = new EvaluateActionHandler(
            _db, _ruleResolver, _lunarCalculatorMock.Object,
            _ganhMenhServiceMock.Object, _canChiCalculatorMock.Object);
    }

    private static ZenTamDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ZenTamDbContext(options);
        SeedTestData(context);
        return context;
    }

    private static void SeedTestData(ZenTamDbContext context)
    {
        // Seed test client for TC-01 (Male, LunarYOB=1996, SolarDOB=1996-05-15)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = SeedClientId,
            Username = "test_user",
            Name = "Test User",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed male client with LunarYOB=1990 for TC-02/TC-07 (KimLau fail test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Male1990Id,
            Username = "male_1990",
            Name = "Male 1990",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed female client with LunarYOB=1996 for TC-03 (Gender filter test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Female1996Id,
            Username = "female_1996",
            Name = "Female 1996",
            PhoneNumber = "0909123458",
            SolarDob = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });

        // Seed male client with LunarYOB=1986 for TC-04/TC-08 (TamTai fail test)
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Male1986Id,
            Username = "male_1986",
            Name = "Male 1986",
            PhoneNumber = "0909123459",
            SolarDob = new DateTime(1986, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed action catalog
        context.ActionCatalog.Add(new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" });

        // Seed action rule mappings for XAY_NHA
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "KimLau",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "HoangOc",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 2
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "TamTai",
            IsMandatory = false,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 3
        });

        context.SaveChanges();
    }

    #region TC-01 — Happy Path: XAY_NHA, Male 1996, TargetYear 2026

    [Fact]
    public async Task TC01_HappyPath_Male1996_TargetYear2026_ReturnsAnToan()
    {
        // Arrange
        var clientId = SeedClientId;
        SetupLunarCalculatorMock(1996);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.IsAllowed.Should().BeTrue();
        response.TotalScore.Should().Be(0);
        response.Verdict.Should().Be("AN_TOAN");
        response.Details.Should().HaveCount(3);

        var kimLau = response.Details.First(d => d.RuleName == "KimLau");
        kimLau.IsPassed.Should().BeTrue();
        kimLau.IsMandatory.Should().BeTrue();
        kimLau.Score.Should().Be(0);

        var hoangOc = response.Details.First(d => d.RuleName == "HoangOc");
        hoangOc.IsPassed.Should().BeTrue();
        hoangOc.IsMandatory.Should().BeTrue();
        hoangOc.Score.Should().Be(0);

        var tamTai = response.Details.First(d => d.RuleName == "TamTai");
        tamTai.IsPassed.Should().BeTrue();
        tamTai.IsMandatory.Should().BeFalse();
        tamTai.Score.Should().Be(0);
    }

    #endregion

    #region TC-02 — Kim Lau Fail: Male 1990, TargetYear 2026

    [Fact]
    public async Task TC02_KimLauFail_Male1990_TargetYear2026_ReturnsCam()
    {
        // Arrange - TC-02 uses Male1990Id per contract Section 4
        var clientId = Male1990Id;
        SetupLunarCalculatorMock(1990);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.IsAllowed.Should().BeFalse();
        response.TotalScore.Should().Be(-5);
        response.Verdict.Should().Be("CAM");

        var kimLau = response.Details.First(d => d.RuleName == "KimLau");
        kimLau.IsPassed.Should().BeFalse();
        kimLau.IsMandatory.Should().BeTrue();
        kimLau.Score.Should().Be(-5);

        var hoangOc = response.Details.First(d => d.RuleName == "HoangOc");
        hoangOc.IsPassed.Should().BeTrue();
        hoangOc.Score.Should().Be(0);

        var tamTai = response.Details.First(d => d.RuleName == "TamTai");
        tamTai.IsPassed.Should().BeTrue();
        tamTai.Score.Should().Be(0);
    }

    #endregion

    #region TC-03 — Gender Filter: Female Client, ActionCode XAY_NHA

    [Fact]
    public async Task TC03_GenderFilter_FemaleClient_ReturnsOnlyTamTai()
    {
        // Arrange - Female1996Id has Gender=Female per seed data
        var clientId = Female1996Id;
        SetupLunarCalculatorMock(1996);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert - Female clients only have TamTai applied (MaleOnly rules filtered)
        response.IsAllowed.Should().BeTrue();
        response.TotalScore.Should().Be(0);
        response.Verdict.Should().Be("AN_TOAN");
        response.Details.Should().HaveCount(1);

        var tamTai = response.Details.First();
        tamTai.RuleName.Should().Be("TamTai");
        tamTai.IsPassed.Should().BeTrue();
        tamTai.IsMandatory.Should().BeFalse();
    }

    #endregion

    #region TC-04 — Tam Tai Fail: YOB chi=Dần(3), TargetYear chi=Thân(9), IsMandatory=false

    [Fact]
    public async Task TC04_TamTaiFail_NonMandatory_Male1986_TargetYear2028_ReturnsCanhBao()
    {
        // Arrange - Male1986Id with TargetYear=2028 triggers TamTai fail
        var clientId = Male1986Id;
        SetupLunarCalculatorMock(1986);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2028
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.IsAllowed.Should().BeTrue(); // TamTai is non-mandatory
        response.TotalScore.Should().Be(-10);
        response.Verdict.Should().Be("CANH_BAO");

        var tamTai = response.Details.First(d => d.RuleName == "TamTai");
        tamTai.IsPassed.Should().BeFalse();
        tamTai.IsMandatory.Should().BeFalse();
        tamTai.Score.Should().Be(-10);

        var kimLau = response.Details.First(d => d.RuleName == "KimLau");
        kimLau.IsPassed.Should().BeTrue();
        kimLau.Score.Should().Be(0);

        var hoangOc = response.Details.First(d => d.RuleName == "HoangOc");
        hoangOc.IsPassed.Should().BeTrue();
        hoangOc.Score.Should().Be(0);
    }

    #endregion

    #region TC-05 & TC-06 — Validation Error and Not Found Exception

    [Fact]
    public async Task TC05_UnknownClient_ThrowsNotFoundException()
    {
        // Arrange - Note: Contract Section 5 changes error message from "User" to "Client"
        var unknownClientId = new Guid("00000000-0000-0000-0000-000000000001");
        SetupLunarCalculatorMock(1996);
        var request = new EvaluateActionRequest
        {
            UserId = unknownClientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act & Assert - Error message now says "Client" per contract Section 5
        var act = async () => await _handler.HandleAsync(request);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Client with Id '{unknownClientId}' was not found.");
    }

    [Fact]
    public async Task TC06_UnknownAction_ThrowsNotFoundException()
    {
        // Arrange
        var clientId = SeedClientId;
        SetupLunarCalculatorMock(1996);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "INVALID",
            TargetYear = 2026
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(request);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Action 'INVALID' was not found.");
    }

    #endregion

    #region TC-07 to TC-12 — Gánh Mệnh Integration Tests

    /// <summary>
    /// TC-07: Verdict = CAM + No RelatedPersons → stays CAM, GanhMenh = null
    /// Note: Male1990Id client has no RelatedPersons seeded
    /// </summary>
    [Fact]
    public async Task TC07_CamVerdict_NoFamilyMembers_GanhMenhIsNull()
    {
        // Arrange: Male1990Id (LunarYOB=1990) with no RelatedPersons
        // KimLau fails: LunarYOB=1990 vs TargetYear=2026 (Canh Thin)
        var clientId = Male1990Id;
        SetupLunarCalculatorMock(1990);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert: Verdict is CAM (KimLau failed)
        response.Verdict.Should().Be("CAM");
        response.GanhMenh.Should().BeNull();
    }

    /// <summary>
    /// TC-08: Verdict = CAM + Family CAN gánh → changes to CANH_BAO, GanhMenh populated
    /// </summary>
    [Fact]
    public async Task TC08_CamVerdict_FamilyCanGanh_VerdictChangesToCanhBao()
    {
        // Arrange: Create a fresh context for this test to avoid tracking issues
        var testDb = CreateInMemoryDbContextWithRelatedPersons(Male1990Id);

        // Setup mock to return a successful Gánh Mệnh result
        var ganhMenhResult = new GanhMenhResult
        {
            CanGanh = true,
            HighestSeverityAmongFamily = 1,
            MemberEvaluations = new List<MemberEvaluation>
            {
                new MemberEvaluation
                {
                    Name = "VỢ",
                    Relationship = RelationshipType.Vo,
                    Verdict = DayVerdict.Binh,
                    Severity = 1
                }
            }
        };

        var handlerWithGanhMenh = CreateHandlerWithGanhMenhMock(testDb, ganhMenhResult, 1990);

        var request = new EvaluateActionRequest
        {
            UserId = Male1990Id,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await handlerWithGanhMenh.HandleAsync(request);
        
        // Wait for fire-and-forget Gánh Mệnh background task to complete
        await Task.Delay(500);

        // Assert: Verdict changed to CANH_BAO because family can gánh
        response.Verdict.Should().Be("CANH_BAO");
        response.GanhMenh.Should().NotBeNull();
        response.GanhMenh!.CanGanh.Should().BeTrue();

        // Cleanup
        await testDb.DisposeAsync();
    }

    /// <summary>
    /// TC-09: Verdict = CAM + Family CANNOT gánh → stays CAM, GanhMenh shows failed attempts
    /// </summary>
    [Fact]
    public async Task TC09_CamVerdict_FamilyCannotGanh_VerdictStaysCam()
    {
        // Arrange: Create a fresh context for this test to avoid tracking issues
        var testDb = CreateInMemoryDbContextWithRelatedPersons(Male1990Id);

        // Setup mock to return failure (no family member can gánh)
        var ganhMenhResult = new GanhMenhResult
        {
            CanGanh = false,
            HighestSeverityAmongFamily = 3,
            MemberEvaluations = new List<MemberEvaluation>
            {
                new MemberEvaluation
                {
                    Name = "VỢ",
                    Relationship = RelationshipType.Vo,
                    Verdict = DayVerdict.Hung,
                    Severity = 3
                }
            }
        };

        var handlerWithGanhMenh = CreateHandlerWithGanhMenhMock(testDb, ganhMenhResult, 1990);

        var request = new EvaluateActionRequest
        {
            UserId = Male1990Id,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await handlerWithGanhMenh.HandleAsync(request);
        
        // Wait for fire-and-forget Gánh Mệnh background task to complete
        await Task.Delay(500);

        // Assert: Verdict stays CAM because no family member can gánh
        response.Verdict.Should().Be("CAM");
        response.GanhMenh.Should().NotBeNull();
        response.GanhMenh!.CanGanh.Should().BeFalse();

        // Cleanup
        await testDb.DisposeAsync();
    }

    /// <summary>
    /// TC-10: Verdict = CANH_BAO → No Gánh Mệnh check (performance optimization)
    /// </summary>
    [Fact]
    public async Task TC10_CanhBaoVerdict_GanhMenhServiceNeverCalled()
    {
        // Arrange: Male1986Id with TargetYear 2028 gives CANH_BAO (TamTai failed, non-mandatory)
        var clientId = Male1986Id;
        SetupLunarCalculatorMock(1986);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2028
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert: Verdict is CANH_BAO (not CAM), so Gánh Mệnh should not be called
        response.Verdict.Should().Be("CANH_BAO");
        _ganhMenhServiceMock.Verify(
            g => g.Evaluate(
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CanChiDay>(),
                It.IsAny<IEnumerable<FamilyMember>>()),
            Times.Never);
    }

    /// <summary>
    /// TC-11: Verdict = AN_TOAN → No Gánh Mệnh check
    /// </summary>
    [Fact]
    public async Task TC11_AnToanVerdict_GanhMenhServiceNeverCalled()
    {
        // Arrange: SeedClientId (Male 1996) with TargetYear 2026 gives AN_TOAN
        var clientId = SeedClientId;
        SetupLunarCalculatorMock(1996);
        var request = new EvaluateActionRequest
        {
            UserId = clientId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert: Verdict is AN_TOAN, Gánh Mệnh should not be called
        response.Verdict.Should().Be("AN_TOAN");
        _ganhMenhServiceMock.Verify(
            g => g.Evaluate(
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CanChiDay>(),
                It.IsAny<IEnumerable<FamilyMember>>()),
            Times.Never);
    }

    /// <summary>
    /// Helper method to setup lunar calculator mock with specific lunar year
    /// </summary>
    private void SetupLunarCalculatorMock(int lunarYear)
    {
        _lunarCalculatorMock.Setup(l => l.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = lunarYear, LunarMonth = 4, LunarDay = 15, IsLeap = false });
    }

    /// <summary>
    /// Creates a test database context with ClientProfile that has RelatedPersons
    /// </summary>
    private static ZenTamDbContext CreateInMemoryDbContextWithRelatedPersons(Guid clientId)
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ZenTamDbContext(options);

        // Seed client with RelatedPersons
        var client = new ClientProfile
        {
            Id = clientId,
            Username = "male_1990",
            Name = "Male 1990",
            PhoneNumber = "0909123457",
            SolarDob = new DateTime(1990, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        client.RelatedPersons.Add(new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Label = "VỢ",
            SolarDob = new DateTime(1992, 3, 20),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });

        context.ClientProfiles.Add(client);

        // Seed action catalog and mappings
        context.ActionCatalog.Add(new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "KimLau",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 1
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "HoangOc",
            IsMandatory = true,
            GenderScope = GenderApplyScope.MaleOnly,
            Tier = RuleTier.Year,
            Priority = 2
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "TamTai",
            IsMandatory = false,
            GenderScope = GenderApplyScope.Both,
            Tier = RuleTier.Year,
            Priority = 3
        });

        context.SaveChanges();
        return context;
    }

    /// <summary>
    /// Helper method to create handler with custom Gánh Mệnh mock result.
    /// Used for TC-08 and TC-09 where we need to control the Gánh Mệnh outcome.
    /// </summary>
    private EvaluateActionHandler CreateHandlerWithGanhMenhMock(ZenTamDbContext db, GanhMenhResult ganhMenhResult, int lunarYear)
    {
        var ganhMenhServiceMock = new Mock<IGanhMenhService>();
        ganhMenhServiceMock.Setup(g => g.Evaluate(
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CanChiDay>(),
            It.IsAny<IEnumerable<FamilyMember>>()))
            .Returns(ganhMenhResult);

        var canChiCalculatorMock = new Mock<ICanChiCalculator>();
        canChiCalculatorMock.Setup(c => c.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Giáp", "Tý"));
        canChiCalculatorMock.Setup(c => c.GetJulianDayNumber(It.IsAny<DateTime>()))
            .Returns(2460000);
        canChiCalculatorMock.Setup(c => c.GetCanChiNgay(It.IsAny<int>()))
            .Returns(new CanChiDay("Giáp", "Tý"));

        var lunarCalculatorMock = new Mock<ILunarCalculatorService>();
        lunarCalculatorMock.Setup(l => l.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = lunarYear, LunarMonth = 4, LunarDay = 15, IsLeap = false });

        var rules = new ISpiritualRule[]
        {
            new KimLauRule(),
            new HoangOcRule(),
            new TamTaiRule(),
            new ThaiTueRule()
        };
        var ruleResolver = new RuleResolver(rules);

        return new EvaluateActionHandler(
            db, ruleResolver, lunarCalculatorMock.Object,
            ganhMenhServiceMock.Object, canChiCalculatorMock.Object);
    }

    #endregion
}
