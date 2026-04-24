using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.Phase1_EvaluateSpiritualAction;

public class EvaluateSpiritualActionTests
{
    private readonly ZenTamDbContext _db;
    private readonly RuleResolver _ruleResolver;
    private readonly Mock<ILunarCalculatorService> _lunarCalculatorMock;
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
            .Returns(new LunarDateContext { LunarYear = 1996, LunarMonth = 4, LunarDay = 15 });
        _handler = new EvaluateActionHandler(_db, _ruleResolver, _lunarCalculatorMock.Object);
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
}
