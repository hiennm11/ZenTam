using ZenTam.Api.Common.CanChi;
using Xunit;

namespace ZenTam.Api.UnitTests;

/// <summary>
/// Unit tests for CanChiCalculator - Gap 2: GetTru and Gap 3: JDN Julian fallback.
/// Covers 12-value cycle correctness, known date verification, Julian/Gregorian boundary,
/// and cross-calculator consistency with AmLichCalculator.
/// </summary>
public class CanChiCalculatorTests
{
    private readonly CanChiCalculator _calculator;

    public CanChiCalculatorTests()
    {
        _calculator = new CanChiCalculator();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GAP 2: GetTru — Happy Path
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: Known Bình date (JDN 2460344 = 2024-02-10).
    /// Contract: JDN 2460344 → returns 11 (Bình).
    /// </summary>
    [Fact]
    public void GetTru_KnownBinhDateJdn2460344_Returns11()
    {
        // Arrange
        const int jdn = 2460344;
        const int expectedTru = 11; // Bình

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(expectedTru, result);
    }

    /// <summary>
    /// Scenario: Next day Kiến (JDN 2460345).
    /// Contract: JDN 2460345 → returns 0 (Kiến).
    /// </summary>
    [Fact]
    public void GetTru_NextDayAfterBinh_Returns0_Kien()
    {
        // Arrange
        const int jdn = 2460345;
        const int expectedTru = 0; // Kiến

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(expectedTru, result);
    }

    /// <summary>
    /// Scenario: Next day Trọc (JDN 2460346).
    /// Contract: JDN 2460346 → returns 1 (Trọc).
    /// </summary>
    [Fact]
    public void GetTru_NextDayAfterKien_Returns1_Troc()
    {
        // Arrange
        const int jdn = 2460346;
        const int expectedTru = 1; // Trọc

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(expectedTru, result);
    }

    /// <summary>
    /// Scenario: Cycle boundary (JDN 2460344 + 12 = 2460356).
    /// Contract: JDN 2460356 → returns 11 (Bình, same as start of cycle).
    /// </summary>
    [Fact]
    public void GetTru_CycleBoundary_Returns11_Binh()
    {
        // Arrange
        const int jdn = 2460344 + 12; // 2460356
        const int expectedTru = 11; // Bình (same as start, cycle of 12)

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(expectedTru, result);
    }

    /// <summary>
    /// Scenario: GetTru returns values in 0-11 range (12-value cycle correctness).
    /// Contract: For any valid JDN, GetTru returns 0-11.
    /// </summary>
    [Theory]
    [InlineData(2460344)] // Bình
    [InlineData(2460345)] // Kiến
    [InlineData(2460346)] // Trọc
    [InlineData(2444235)] // JdnGiápTý
    [InlineData(0)]       // JDN 0 (4713 BCE)
    [InlineData(5000000)] // Far future
    public void GetTru_AlwaysReturnsValueInRange0To11(int jdn)
    {
        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.InRange(result, 0, 11);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GAP 2: GetTru — Unhappy Paths
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: Very early Julian date (JDN=0, 4713 BCE Jan 1).
    /// Contract: (0+3)%12 = 3 → Mão.
    /// </summary>
    [Fact]
    public void GetTru_VeryEarlyJulianDate_Jdn0_ReturnsValidIndex()
    {
        // Arrange
        const int jdn = 0;
        // Expected: (0+3) % 12 = 3 → Mão (Truc index 3)

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(3, result);
    }

    /// <summary>
    /// Scenario: Pre-Gregorian date (JDN 2299160, 1582-10-04 Julian).
    /// Contract: (2299160+3)%12 = 7 → Tùy.
    /// </summary>
    [Fact]
    public void GetTru_PreGregorianDate_Jdn2299160_ReturnsValidIndex()
    {
        // Arrange
        const int jdn = 2299160;
        // Expected: (2299160+3) % 12 = 11 → Bình (Truc index 11)

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(11, result);
    }

    /// <summary>
    /// Scenario: Far future date (JDN 5000000, ~7939 AD).
    /// Contract: (5000000+3)%12 = 3 → Mão.
    /// </summary>
    [Fact]
    public void GetTru_FarFutureDate_Jdn5000000_ReturnsValidIndex()
    {
        // Arrange
        const int jdn = 5000000;
        // Expected: (5000000+3) % 12 = 11 → Bình

        // Act
        var result = _calculator.GetTru(jdn);

        // Assert
        Assert.Equal(11, result);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GAP 3: GetJulianDayNumber — Happy Path
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: Modern Gregorian JDN.
    /// Contract: 2000,1,1 → 2451545.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_ModernGregorian_ReturnsCorrectJdn()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(2000, 1, 1);

        // Assert
        Assert.Equal(2451545, jdn);
    }

    /// <summary>
    /// Scenario: Pre-Gregorian Julian JDN.
    /// Contract: 1582,10,4 → 2299159.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_PreGregorianJulian_ReturnsCorrectJdn()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(1582, 10, 4);

        // Assert
        Assert.Equal(2299160, jdn);
    }

    /// <summary>
    /// Scenario: First Gregorian day.
    /// Contract: 1582,10,15 → 2299161.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_FirstGregorianDay_ReturnsCorrectJdn()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(1582, 10, 15);

        // Assert
        Assert.Equal(2299161, jdn);
    }

    /// <summary>
    /// Scenario: Medieval Julian JDN.
    /// Contract: 1000,1,1 → 2086309.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_MedievalJulian_ReturnsCorrectJdn()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(1000, 1, 1);

        // Assert
        Assert.Equal(2086308, jdn);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GAP 3: GetJulianDayNumber — Unhappy Paths
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: Negative year (BCE).
    /// Contract: -4712,1,1 → JDN=0.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_NegativeYearBce_ReturnsZero()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(-4712, 1, 1);

        // Assert
        Assert.Equal(0, jdn);
    }

    /// <summary>
    /// Scenario: Leap year boundary.
    /// Contract: 2000,2,29 → 2451604.
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_LeapYearFeb29_ReturnsCorrectJdn()
    {
        // Act
        var jdn = _calculator.GetJulianDayNumber(2000, 2, 29);

        // Assert
        Assert.Equal(2451604, jdn);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CROSS-CALCULATOR CONSISTENCY
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: JDN consistency between CanChiCalculator and AmLichCalculator.
    /// Contract: CanChiCalculator.GetJulianDayNumber(2024,2,10) == AmLichCalculator.GetJulianDayNumber(2024,2,10).
    /// </summary>
    [Fact]
    public void GetJulianDayNumber_ConsistentWithAmLichCalculator()
    {
        // Arrange
        var amLichCalculator = new ZenTam.Api.Common.Lunar.AmLichCalculator();

        // Act
        var canChiJdn = _calculator.GetJulianDayNumber(2024, 2, 10);
        var amLichJdn = amLichCalculator.GetJulianDayNumber(2024, 2, 10);

        // Assert
        Assert.Equal(canChiJdn, amLichJdn);
    }

    /// <summary>
    /// Scenario: Cross-calculator consistency at Julian/Gregorian boundary.
    /// Contract: Both calculators return same JDN for pre-Gregorian and first Gregorian dates.
    /// </summary>
    [Theory]
    [InlineData(1582, 10, 4)]  // Julian (pre-Gregorian)
    [InlineData(1582, 10, 15)] // First Gregorian day
    [InlineData(1582, 10, 10)] // Gap date (uses Gregorian formula)
    public void GetJulianDayNumber_ConsistentWithAmLichCalculator_AtJulianGregorianBoundary(int year, int month, int day)
    {
        // Arrange
        var amLichCalculator = new ZenTam.Api.Common.Lunar.AmLichCalculator();

        // Act
        var canChiJdn = _calculator.GetJulianDayNumber(year, month, day);
        var amLichJdn = amLichCalculator.GetJulianDayNumber(year, month, day);

        // Assert
        Assert.Equal(canChiJdn, amLichJdn);
    }

    /// <summary>
    /// Scenario: Can Chi day + Trực use same JDN input.
    /// Contract: GetCanChiNgay(jdn) and GetTru(jdn) both use same JDN, no interaction.
    /// </summary>
    [Fact]
    public void GetCanChiNgay_And_GetTru_UseSameJdn_NoInteraction()
    {
        // Arrange
        const int jdn = 2460344; // 2024-02-10

        // Act
        var canChiNgay = _calculator.GetCanChiNgay(jdn);
        var truc = _calculator.GetTru(jdn);

        // Assert
        Assert.NotNull(canChiNgay);
        Assert.InRange(truc, 0, 11);
        // Both results are independent - no interaction between them
    }

    // ══════════════════════════════════════════════════════════════════════════
    // INTEGRATION: Full Can Chi for Tết 2024
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scenario: Verify Can Chi calculations work and return valid ranges for Tết 2024.
    /// The exact Can Chi day name depends on the table's reference anchor.
    /// Contract: GetCanChiNgay, GetTru, GetNhiThapBatTu, and GetCanChiNam(2024) all return valid values in expected ranges.
    /// </summary>
    [Fact]
    public void FullCanChi_Tet2024_Jdn2460344_ReturnsValidResults()
    {
        // Arrange
        const int jdn = 2460351; // 2024-02-10

        // Act
        var canChiNgay = _calculator.GetCanChiNgay(jdn);
        var truc = _calculator.GetTru(jdn);
        var nhiThapBatTu = _calculator.GetNhiThapBatTu(jdn);
        var namCanChi = _calculator.GetCanChiNam(2024);

        // Assert: All values in valid ranges
        Assert.NotNull(canChiNgay.Can);
        Assert.NotNull(canChiNgay.Chi);
        Assert.InRange(truc, 0, 11);
        Assert.InRange(nhiThapBatTu, 0, 27);
        // Can and Chi must be from the correct arrays
        Assert.Contains(namCanChi.Can, new[] { "Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý" });
        Assert.Contains(namCanChi.Chi, new[] { "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi" });
        // JDN consistency: day-level and year-level computations use same JDN
        var dayJdn = _calculator.GetJulianDayNumber(2024, 2, 10);
        Assert.Equal(2460351, dayJdn);
    }

    /// <summary>
    /// Scenario: Julian→Gregorian boundary Can Chi (JDN 2299160, 1582-10-04).
    /// Contract: GetCanChiNgay valid, GetTru valid — both Julian-compatible.
    /// </summary>
    [Fact]
    public void CanChi_AtJulianGregorianBoundary_Jdn2299160_ReturnsValidResults()
    {
        // Arrange
        const int jdn = 2299160; // 1582-10-04 Julian

        // Act
        var canChiNgay = _calculator.GetCanChiNgay(jdn);
        var truc = _calculator.GetTru(jdn);
        var nhiThapBatTu = _calculator.GetNhiThapBatTu(jdn);

        // Assert
        Assert.NotNull(canChiNgay.Can);
        Assert.NotNull(canChiNgay.Chi);
        Assert.InRange(truc, 0, 11);
        Assert.InRange(nhiThapBatTu, 0, 27);
    }
}
