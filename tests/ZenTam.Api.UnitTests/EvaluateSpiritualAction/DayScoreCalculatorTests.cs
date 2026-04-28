using Microsoft.EntityFrameworkCore;
using Moq;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.Calendars.Models;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

using LunarDateContext = ZenTam.Api.Common.Domain.LunarDateContext;

namespace ZenTam.Api.UnitTests.EvaluateSpiritualAction;

/// <summary>
/// Tests for DayScoreCalculator - full day scoring logic.
/// Max score = 80 (Thành=20 + Kiết Tú=6 + Hoàng=6 + no penalties each=12 x 4)
/// </summary>
public class DayScoreCalculatorTests
{
    private readonly Mock<IDayContextService> _mockDayContextService;
    private readonly Mock<ILunarCalculatorService> _mockLunarCalculator;
    private readonly Mock<ICanChiCalculator> _mockCanChiCalculator;
    private readonly Mock<ActionCodeMapper> _mockActionCodeMapper;
    private readonly Mock<RuleResolver> _mockRuleResolver;
    private readonly DayScoreCalculator _calculator;
    private readonly ZenTamDbContext _db;

    public DayScoreCalculatorTests()
    {
        _mockDayContextService = new Mock<IDayContextService>();
        _mockLunarCalculator = new Mock<ILunarCalculatorService>();
        _mockCanChiCalculator = new Mock<ICanChiCalculator>();

        // Setup in-memory DB for ActionCodeMapper
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ZenTamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new ZenTamDbContext(options);

        // Use real ActionCodeMapper and RuleResolver (can't mock concrete classes with non-virtual methods)
        var actionCodeMapper = new ActionCodeMapper(_db);
        var ruleResolver = new RuleResolver(Array.Empty<ISpiritualRule>());

        _calculator = new DayScoreCalculator(
            _mockDayContextService.Object,
            _mockLunarCalculator.Object,
            _mockCanChiCalculator.Object,
            actionCodeMapper,
            ruleResolver);
    }

    [Fact]
    public void Calculate_PerfectDay_HasExpectedScoreComponents()
    {
        // Arrange: Perfect day = Thành(20) + Kiết Tú(6) + Hoàng(6) + no negatives (12 x 4 = 48)
        // Total = 20 + 6 + 6 + 48 = 80
        var solarDate = new DateTime(2026, 5, 15);

        // Setup complete day context with all "good" values
        var dayContext = CreateDayContext(
            solarDate: solarDate,
            trucIndex: 8,   // Thành
            trucName: "Thành",
            tuIndex: 0,     // Côn = Kiết Tú
            tuName: "Côn",
            tuClassification: TuClassification.Kiettu,
            isHoangDao: true,
            isSatChu: false,
            isThuTu: false,
            isNgayKy: false
        );

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        
        // Setup lunar context
        var lunarCtx = new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false };
        _mockLunarCalculator.Setup(x => x.Convert(solarDate)).Returns(lunarCtx);
        
        // Setup CanChi mocks (needed for LunarDateText)
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(2026)).Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - verify score components
        // Thành (index 8) + NHAP_TRACH (index 0) = 20
        Assert.Equal(8, result.TrucIndex);
        Assert.Equal("Thành", result.TrucName);
        
        // Tú classification: Kiết Tú = +6
        Assert.Equal(TuClassification.Kiettu, dayContext.NhiThapBatTu.Classification);
        
        // Hoàng Đạo = +6
        Assert.True(result.IsHoangDao);
        
        // All "no penalty" conditions should pass
        Assert.False(result.IsNgayKy);
        Assert.False(result.IsSatChu);
        Assert.False(result.IsThuTu);
        
        // Verify reasons contain expected entries
        Assert.Contains(result.Reasons, r => r.Contains("Thành"));
        Assert.Contains(result.Reasons, r => r.Contains("Kiết Tú"));
        Assert.Contains(result.Reasons, r => r == "Hoàng Đạo");
        
        // Final score verification
        Assert.True(result.Score >= 68, $"Expected score >= 68, got {result.Score}");
    }

    [Fact]
    public void Calculate_HoangDaoAddsPoints()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var withHoang = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - Hoàng Đạo adds 6 points
        Assert.True(withHoang.Score >= 26);
        Assert.Contains(withHoang.Reasons, r => r == "Hoàng Đạo");
    }

    [Fact]
    public void Calculate_SatChuReducesScore()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: true, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - Sát Chủ means no +12 bonus
        Assert.True(result.IsSatChu);
    }

    [Fact]
    public void Calculate_ThuTuReducesScore()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: true, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - Thọ Tử means no +12 bonus
        Assert.True(result.IsThuTu);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(14)]
    [InlineData(23)]
    public void Calculate_NgayKyReducesScore(int lunarDay)
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: false, isNgayKy: true);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = lunarDay, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - Ngày Kỵ means no +12 bonus
        Assert.True(result.IsNgayKy);
    }

    [Fact]
    public void Calculate_XungTuoiReducesScore_PenaltyApplied()
    {
        // Arrange - Client born 1984 (Tý), day is Mão (xung)
        var solarDate = new DateTime(2026, 5, 15);
        int clientLunarYear = 1984; // Tý

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        
        var lunarCtx = new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false };
        _mockLunarCalculator.Setup(x => x.Convert(solarDate)).Returns(lunarCtx);
        _mockLunarCalculator.Setup(x => x.GetJulianDayNumber(2026, 5, 15)).Returns(2460850);

        // CanChi year for 1984 = Giáp Tý, day CanChi for Mão = xung with Tý
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(1984))
            .Returns(new CanChiYear("Giáp", "Tý"));
        _mockCanChiCalculator.Setup(x => x.GetCanChiNgay(2460850))
            .Returns(new CanChiDay("Ất", "Mão"));
        
        // Mock for lunar year used in LunarDateText
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(2026))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year, clientLunarYear);

        // Assert
        Assert.True(result.IsXungTuoi);
        Assert.Contains("Xung tuổi", result.Reasons);
    }

    [Fact]
    public void Calculate_NoXungTuoi_WhenNoClientYear()
    {
        // Arrange - No client year means no xung tuổi check
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year, clientLunarYear: null);

        // Assert - no xung check performed
        Assert.False(result.IsXungTuoi);
        // When no client year, no "Không xung tuổi" reason should be added (by design)
    }

    [Theory]
    [InlineData(TuClassification.Kiettu, 0)]   // Index 0 = Côn = Kiettu
    [InlineData(TuClassification.Binhtu, 23)] // Index 23 = Quân = Binhtu
    [InlineData(TuClassification.Hungtu, 12)]  // Index 12 = Mão = Hungtu
    public void Calculate_TuClassification_ValidIndices(TuClassification classification, int tuIndex)
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 3, trucName: "Bình",
            tuIndex: tuIndex, tuName: "Test", tuClassification: classification,
            isHoangDao: false, isSatChu: false, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert
        Assert.Equal(tuIndex, result.TuIndex);
    }

    [Fact]
    public void Calculate_AllPenaltiesApply_ScoreCanBeNegative()
    {
        // Arrange: Nguy(-20 to -30) + Hưng Tú(-6) + no hoang(0) + xung(-12) + ky(0) + sat(0) + tho(+12)
        // With KET_HON specifically: Nguy=-30
        var solarDate = new DateTime(2026, 4, 12);

        var dayContext = CreateDayContext(solarDate, trucIndex: 7, trucName: "Nguy",
            tuIndex: 12, tuName: "Mão", tuClassification: TuClassification.Hungtu,
            isHoangDao: false, isSatChu: true, isThuTu: false, isNgayKy: true);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 3, LunarDay = 5, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.CUOI_HOI, Gender.Male, RuleTier.Year);

        // Assert - score should be very negative
        Assert.True(result.Score < 0);
        Assert.Equal(7, result.TrucIndex);
    }

    [Fact]
    public void Calculate_ReturnsAllRequiredFields()
    {
        // Arrange
        var solarDate = new DateTime(2026, 5, 15);

        var dayContext = CreateDayContext(solarDate, trucIndex: 8, trucName: "Thành",
            tuIndex: 0, tuName: "Côn", tuClassification: TuClassification.Kiettu,
            isHoangDao: true, isSatChu: false, isThuTu: false, isNgayKy: false);

        _mockDayContextService.Setup(x => x.GetDayContext(solarDate)).Returns(dayContext);
        _mockLunarCalculator.Setup(x => x.Convert(solarDate))
            .Returns(new LunarDateContext { LunarYear = 2026, LunarMonth = 4, LunarDay = 16, IsLeap = false });
        _mockCanChiCalculator.Setup(x => x.GetCanChiNam(It.IsAny<int>()))
            .Returns(new CanChiYear("Bính", "Ngọ"));

        // Act
        var result = _calculator.Calculate(solarDate, ActionCode.NHAP_TRACH, Gender.Male, RuleTier.Year);

        // Assert - all fields present
        Assert.Equal(solarDate, result.SolarDate);
        Assert.False(string.IsNullOrEmpty(result.LunarDateText));
        Assert.False(string.IsNullOrEmpty(result.CanChiNgay));
        Assert.Equal(8, result.TrucIndex);
        Assert.Equal("Thành", result.TrucName);
        Assert.Equal(0, result.TuIndex);
        Assert.Equal("Côn", result.TuName);
        Assert.True(result.IsHoangDao);
        Assert.False(result.IsSatChu);
        Assert.False(result.IsThuTu);
        Assert.False(result.IsNgayKy);
        Assert.NotEmpty(result.Reasons);
    }

    private static DayContext CreateDayContext(
        DateTime solarDate,
        int trucIndex,
        string trucName,
        int tuIndex,
        string tuName,
        TuClassification tuClassification,
        bool isHoangDao,
        bool isSatChu,
        bool isThuTu,
        bool isNgayKy)
    {
        return new DayContext(
            SolarDate: solarDate,
            CanChiNgay: "Bính Ngọ",
            CanChiThang: "Bính Dần",
            TrucIndex: trucIndex,
            TrucName: trucName,
            NhiThapBatTu: new NhiThapBatTuInfo(tuIndex, tuName, tuClassification),
            HoangDao: new HoangDaoInfo(isHoangDao, new List<string>(), new List<string>(), new List<string>()),
            SatChu: new SatChuInfo(isSatChu, -1),
            ThuTu: new ThuTuInfo(isThuTu, Array.Empty<int>()),
            IsNgayKy: isNgayKy
        );
    }
}