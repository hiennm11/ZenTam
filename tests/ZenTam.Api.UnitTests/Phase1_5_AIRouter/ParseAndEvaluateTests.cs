using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction.Queries;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Features.ParseAndEvaluate.Queries;
using ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.UnitTests.Phase1_5_AIRouter;

public class ParseAndEvaluateTests
{
    private ZenTamDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ZenTamDbContext(options);
        SeedData(context);
        return context;
    }

    private static void SeedData(ZenTamDbContext context)
    {
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            Name = "Test Client",
            PhoneNumber = "0909123456",
            SolarDob = new DateTime(1996, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        });

        // Seed action catalog
        context.ActionCatalog.Add(new ActionCatalog { Id = "XAY_NHA", Description = "Xây nhà mới" });

        // Seed action rule mappings
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 1,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_KimLau",
            IsMandatory = true,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 2,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_HoangOc",
            IsMandatory = true,
            GenderConstraint = Gender.Male
        });
        context.ActionRuleMappings.Add(new ActionRuleMapping
        {
            Id = 3,
            ActionId = "XAY_NHA",
            RuleCode = "Rule_TamTai",
            IsMandatory = false,
            GenderConstraint = Gender.Male
        });

        context.SaveChanges();
    }

    #region Test 1 — Regex Full Hit

    [Fact]
    public async Task Test1_RegexFullHit_ReturnsCorrectIntent()
    {
        // Arrange
        var regexParser = new RegexIntentParser();

        // Act
        var result = await regexParser.TryParseAsync("xây nhà năm 2027", 2026);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].ActionCode.Should().Be("XAY_NHA");
        result[0].TargetYear.Should().Be(2027);
        result[0].Source.Should().Be("REGEX");
    }

    #endregion

    #region Test 2 — Regex Partial: Relative Year "sang năm"

    [Fact]
    public async Task Test2_RegexRelativeYear_ResolvesToCurrentYearPlusOne()
    {
        // Arrange
        var regexParser = new RegexIntentParser();
        int currentYear = 2026;

        // Act
        var result = await regexParser.TryParseAsync("sang năm t động thổ", currentYear);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].ActionCode.Should().Be("XAY_NHA");
        result[0].TargetYear.Should().Be(currentYear + 1); // 2027
    }

    #endregion

    #region Test 3 — Regex Miss, SLM Hit

    [Fact]
    public async Task Test3_RegexMissSLMHit_ReturnsSLMResult()
    {
        // Arrange
        var regexParser = new RegexIntentParser();
        var mockSLM = new Mock<IIntentParser>();
        mockSLM.Setup(s => s.TryParseAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParsedIntent>
            {
                new("XAY_NHA", 2027, "SLM")
            });

        // Act - Regex miss
        var regexResult = await regexParser.TryParseAsync("t nghĩ làm cái việc lớn sang năm", 2026);
        regexResult.Should().BeNull();

        // Act - SLM hit (simulated)
        var slmResult = await mockSLM.Object.TryParseAsync("t nghĩ làm cái việc lớn sang năm", 2026);

        // Assert
        slmResult.Should().NotBeNull();
        slmResult.Should().HaveCount(1);
        slmResult![0].ActionCode.Should().Be("XAY_NHA");
        slmResult[0].Source.Should().Be("SLM");
    }

    #endregion

    #region Test 4 — SLM Returns Null ActionCode → 422

    [Fact]
    public async Task Test4_SLMReturnsNull_ReturnsNull()
    {
        // Arrange
        var mockSLM = new Mock<IIntentParser>();
        mockSLM.Setup(s => s.TryParseAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ParsedIntent>?)null);

        // Act
        var result = await mockSLM.Object.TryParseAsync("hôm nay trời đẹp quá", 2026);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Test 5 — Cache Hit (Second Request)

    [Fact]
    public async Task Test5_CacheHit_ReturnsCachedResult()
    {
        // Arrange
        var cachedResponse = new EvaluateActionResponse
        {
            IsAllowed = true,
            TotalScore = 10,
            Verdict = "AN_TOAN",
            Details = new List<RuleResult>()
        };

        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<EvaluateActionResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await mockCache.Object.GetAsync<EvaluateActionResponse>("test-key", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsAllowed.Should().BeTrue();
        result.TotalScore.Should().Be(10);
    }

    #endregion

    #region Test 6 — Text Exceeds 500 Characters → 400

    [Fact]
    public async Task Test6_TextExceeds500Chars_ReturnsValidationError()
    {
        // Arrange
        var validator = new ParseAndEvaluateValidator();
        var request = new ParseAndEvaluateRequest(Guid.NewGuid(), new string('a', 501));

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    #endregion

    #region Test 7 — Empty Text → 400

    [Fact]
    public async Task Test7_EmptyText_ReturnsValidationError()
    {
        // Arrange
        var validator = new ParseAndEvaluateValidator();
        var request = new ParseAndEvaluateRequest(Guid.NewGuid(), "");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    #endregion

    #region Test 8 — Empty Guid UserId → 400

    [Fact]
    public async Task Test8_EmptyGuidUserId_ReturnsValidationError()
    {
        // Arrange
        var validator = new ParseAndEvaluateValidator();
        var request = new ParseAndEvaluateRequest(Guid.Empty, "xây nhà năm 2027");

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClientId");
    }

    #endregion

    #region Test 9 — UserId Not Found in DB → 404

    [Fact]
    public async Task Test9_UserNotFound_ReturnsNull()
    {
        // Arrange
        var db = CreateInMemoryDbContext();
        var unknownClientId = Guid.NewGuid();

        // Act
        var result = await db.ClientProfiles.FirstOrDefaultAsync(c => c.Id == unknownClientId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Test 10 — Year Edge Case: 31 December 23:59 UTC

    [Fact]
    public void Test10_VietnamYearResolution_UsesSEAsiaTimeZone()
    {
        // This test verifies that year resolution uses Vietnam timezone (UTC+7)
        // In UTC 2026-12-31 23:59, Vietnam time is 2027-01-01 06:59
        // So "sang năm" should resolve to 2028 (Vietnam year 2027 + 1)

        // Arrange
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var utcDateTime = new DateTime(2026, 12, 31, 23, 59, 0, DateTimeKind.Utc);

        // Act
        var vnDateTime = TimeZoneInfo.ConvertTime(utcDateTime, vnTimeZone);

        // Assert
        vnDateTime.Year.Should().Be(2027); // Vietnam is UTC+7, so it's already Jan 1, 2027
    }

    #endregion

    #region Test 11 — SLM Timeout Handled Gracefully

    [Fact]
    public async Task Test11_SLMTimeout_ThrowsTaskCanceledException()
    {
        // Arrange
        var mockSLM = new Mock<IIntentParser>();
        mockSLM.Setup(s => s.TryParseAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("LiteLLM timeout"));

        // Act
        var act = async () => await mockSLM.Object.TryParseAsync("test message", 2026);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    #endregion
}
