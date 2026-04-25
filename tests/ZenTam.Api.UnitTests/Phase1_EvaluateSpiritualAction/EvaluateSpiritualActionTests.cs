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
        _lunarCalculatorMock.Setup(l => l.Convert(It.IsAny<DateTime>()))
            .Returns(new LunarDateContext { LunarYear = 1996, LunarMonth = 4, LunarDay = 15, IsLeap = false });
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
        // Seed test user for TC-01 (Male, LunarYOB=1996) - to match blueprint
        context.Users.Add(new User
        {
            Id = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675"),
            Username = "test_user",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1996
        });

        // Seed male user with LunarYOB=1990 for TC-02
        context.Users.Add(new User
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Username = "male_1990",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1990
        });

        // Seed female user with LunarYOB=1996 for TC-03
        context.Users.Add(new User
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Username = "female_1996",
            Gender = Gender.Female,
            SolarDOB = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1996
        });

        // Seed male user with LunarYOB=1986 for TC-04
        context.Users.Add(new User
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Username = "male_1986",
            Gender = Gender.Male,
            SolarDOB = new DateTime(1986, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            LunarYOB = 1986
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
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "HoangOc",
            IsMandatory = true,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "TamTai",
            IsMandatory = false,
            GenderConstraint = null  // No gender constraint - applies to all genders
        });

        context.SaveChanges();
    }

    #region TC-01 — Happy Path: XAY_NHA, Male 1996, TargetYear 2026

    [Fact]
    public async Task TC01_HappyPath_Male1996_TargetYear2026_ReturnsAnToan()
    {
        // Arrange
        var userId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
        // Arrange
        var userId = new Guid("11111111-1111-1111-1111-111111111111");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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

    #region TC-03 — Gender Filter: Female User, ActionCode XAY_NHA

    [Fact]
    public async Task TC03_GenderFilter_FemaleUser_ReturnsOnlyTamTai()
    {
        // Arrange
        var userId = new Guid("22222222-2222-2222-2222-222222222222");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
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
        // Arrange
        var userId = new Guid("33333333-3333-3333-3333-333333333333");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    public async Task TC06_UnknownUser_ThrowsNotFoundException()
    {
        // Arrange
        var unknownUserId = new Guid("00000000-0000-0000-0000-000000000001");
        var request = new EvaluateActionRequest
        {
            UserId = unknownUserId,
            ActionCode = "XAY_NHA",
            TargetYear = 2026
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(request);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with Id '{unknownUserId}' was not found.");
    }

    [Fact]
    public async Task TC07_UnknownAction_ThrowsNotFoundException()
    {
        // Arrange
        var userId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
            ActionCode = "INVALID",
            TargetYear = 2026
        };

        // Act & Assert
        var act = async () => await _handler.HandleAsync(request);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Action 'INVALID' was not found.");
    }

    #endregion

    #region TC-08 to TC-11 — Gánh Mệnh Integration Tests

    /// <summary>
    /// TC-08: Verdict = CAM + No family → stays CAM, GanhMenh = null
    /// </summary>
    [Fact]
    public async Task TC08_CamVerdict_NoFamilyMembers_GanhMenhIsNull()
    {
        // Arrange: User 1990 with no ClientProfile (or no RelatedPersons)
        var userId = new Guid("11111111-1111-1111-1111-111111111111");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    /// TC-09: Verdict = CAM + Family CAN gánh → changes to CANH_BAO, GanhMenh populated
    /// </summary>
    [Fact]
    public async Task TC09_CamVerdict_FamilyCanGanh_VerdictChangesToCanhBao()
    {
        // Arrange: Set up user with ClientProfile and RelatedPersons
        var userId = new Guid("11111111-1111-1111-1111-111111111111");
        var clientId = userId; // ClientProfile.Id matches User.Id

        var clientProfile = new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "123456789",
            SolarDob = new DateTime(1990, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        clientProfile.RelatedPersons.Add(new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Label = "VỢ",
            SolarDob = new DateTime(1992, 3, 20),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });

        _db.ClientProfiles.Add(clientProfile);
        await _db.SaveChangesAsync();

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

        var handlerWithGanhMenh = CreateHandlerWithGanhMenhMock(ganhMenhResult);

        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    }

    /// <summary>
    /// TC-10: Verdict = CAM + Family CANNOT gánh → stays CAM, GanhMenh shows failed attempts
    /// </summary>
    [Fact]
    public async Task TC10_CamVerdict_FamilyCannotGanh_VerdictStaysCam()
    {
        // Arrange: Set up user with ClientProfile and RelatedPersons with bad zodiac
        var userId = new Guid("11111111-1111-1111-1111-111111111111");
        var clientId = userId;

        var clientProfile = new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "123456789",
            SolarDob = new DateTime(1990, 5, 15),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };
        clientProfile.RelatedPersons.Add(new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Label = "VỢ",
            SolarDob = new DateTime(1992, 3, 20),
            Gender = Gender.Female,
            CreatedAt = DateTime.UtcNow
        });

        _db.ClientProfiles.Add(clientProfile);
        await _db.SaveChangesAsync();

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

        var handlerWithGanhMenh = CreateHandlerWithGanhMenhMock(ganhMenhResult);

        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    }

    /// <summary>
    /// TC-11: Verdict = CANH_BAO → No Gánh Mệnh check (performance optimization)
    /// </summary>
    [Fact]
    public async Task TC11_CanhBaoVerdict_GanhMenhServiceNeverCalled()
    {
        // Arrange: User 1986 with TargetYear 2028 gives CANH_BAO (TamTai failed, non-mandatory)
        var userId = new Guid("33333333-3333-3333-3333-333333333333");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    /// TC-12: Verdict = AN_TOAN → No Gánh Mệnh check
    /// </summary>
    [Fact]
    public async Task TC12_AnToanVerdict_GanhMenhServiceNeverCalled()
    {
        // Arrange: User 1996 with TargetYear 2026 gives AN_TOAN
        var userId = new Guid("3c7be808-02c1-4f24-85e1-26f0f2455675");
        var request = new EvaluateActionRequest
        {
            UserId = userId,
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
    /// Helper method to create handler with custom Gánh Mệnh mock result.
    /// Used for TC-09 and TC-10 where we need to control the Gánh Mệnh outcome.
    /// </summary>
    private EvaluateActionHandler CreateHandlerWithGanhMenhMock(GanhMenhResult ganhMenhResult)
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
            .Returns(new LunarDateContext { LunarYear = 1996, LunarMonth = 4, LunarDay = 15, IsLeap = false });

        return new EvaluateActionHandler(
            _db, _ruleResolver, lunarCalculatorMock.Object,
            ganhMenhServiceMock.Object, canChiCalculatorMock.Object);
    }

    #endregion
}
