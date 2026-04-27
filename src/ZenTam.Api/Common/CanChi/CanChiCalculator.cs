using ZenTam.Api.Common.CanChi.Models;
using ZenTam.Api.Features.Calendars.Data;
using ZenTam.Api.Features.Calendars.Services;

namespace ZenTam.Api.Common.CanChi;

/// <summary>
/// Can Chi (Stem-Branch) Calculator implementation.
/// Stateless, thread-safe, singleton-ready.
/// </summary>
public class CanChiCalculator(
    ZenTam.Api.Common.Lunar.ILunarCalculatorService lunarCalculator,
    ISolarTermCalculator solarTermCalculator) : ICanChiCalculator
{
    // 10 Heavenly Stems
    private static readonly string[] Cans =
    {
        "Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý"
    };

    // 12 Earthly Branches
    private static readonly string[] Chis =
    {
        "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"
    };

    // 60-value cyclic table starting from 1984-01-01 = Giáp Tý
    private static readonly (string Can, string Chi)[] CanChiNgayTable =
    [
        // Index 0–11: Giáp Tý cycle start
        ("Giáp", "Tý"), ("Ất", "Sửu"), ("Bính", "Dần"), ("Đinh", "Mão"),
        ("Mậu", "Thìn"), ("Kỷ", "Tỵ"), ("Canh", "Ngọ"), ("Tân", "Mùi"),
        ("Nhâm", "Thân"), ("Quý", "Dậu"), ("Giáp", "Tuất"), ("Ất", "Hợi"),
        // Index 12–23
        ("Bính", "Tý"), ("Đinh", "Sửu"), ("Mậu", "Dần"), ("Kỷ", "Mão"),
        ("Canh", "Thìn"), ("Tân", "Tỵ"), ("Nhâm", "Ngọ"), ("Quý", "Mùi"),
        ("Giáp", "Thân"), ("Ất", "Dậu"), ("Bính", "Tuất"), ("Đinh", "Hợi"),
        // Index 24–35
        ("Mậu", "Tý"), ("Kỷ", "Sửu"), ("Canh", "Dần"), ("Tân", "Mão"),
        ("Nhâm", "Thìn"), ("Quý", "Tỵ"), ("Giáp", "Ngọ"), ("Ất", "Mùi"),
        ("Bính", "Thân"), ("Đinh", "Dậu"), ("Mậu", "Tuất"), ("Kỷ", "Hợi"),
        // Index 36–47
        ("Canh", "Tý"), ("Tân", "Sửu"), ("Nhâm", "Dần"), ("Quý", "Mão"),
        ("Giáp", "Thìn"), ("Ất", "Tỵ"), ("Bính", "Ngọ"), ("Đinh", "Mùi"),
        ("Mậu", "Thân"), ("Kỷ", "Dậu"), ("Canh", "Tuất"), ("Tân", "Hợi"),
        // Index 48–59
        ("Nhâm", "Tý"), ("Quý", "Sửu"), ("Giáp", "Dần"), ("Ất", "Mão"),
        ("Bính", "Thìn"), ("Đinh", "Tỵ"), ("Mậu", "Ngọ"), ("Kỷ", "Mùi"),
        ("Canh", "Thân"), ("Tân", "Dậu"), ("Nhâm", "Tuất"), ("Quý", "Hợi")
    ];

    // Nhị Thập Bát Tú names (28 values)
    private static readonly string[] NhiThapBatTuNames =
    [
        "Côn", "Đẩu", "Nữ", "Bộc", "Tham", "Chu", "Xà", "Tinh", "Trương", "Dực",
        "Chẩn", "Vĩ", "Mão", "Tất", "Mão", "Chu", "Sâm", "Tỉ", "Thư", "Vị",
        "Trủy", "Nguy", "Dương", "Quân", "Lâu", "Vệ", "Cư", "Mỹ"
    ];

    // Can-dependent month Can table (month 1 = index 0)
    // Row 0: Giáp (0), Row 1: Ất (1), ... Row 9: Quý (9)
    // Col 0=month1(Giáp), Col1=month2(Ất), ... Col11=month12(Quý)
    private static readonly int[,] MonthCanIndexTable = new int[,]
    {
        {0, 2, 4, 6, 8, 0, 2, 4, 6, 8, 0, 2}, // Giáp
        {1, 3, 5, 7, 9, 1, 3, 5, 7, 9, 1, 3}, // Ất
        {2, 4, 6, 8, 0, 2, 4, 6, 8, 0, 2, 4}, // Bính
        {3, 5, 7, 9, 1, 3, 5, 7, 9, 1, 3, 5}, // Đinh
        {4, 6, 8, 0, 2, 4, 6, 8, 0, 2, 4, 6}, // Mậu
        {5, 7, 9, 1, 3, 5, 7, 9, 1, 3, 5, 7}, // Kỷ
        {6, 8, 0, 2, 4, 6, 8, 0, 2, 4, 6, 8}, // Canh
        {7, 9, 1, 3, 5, 7, 9, 1, 3, 5, 7, 9}, // Tân
        {8, 0, 2, 4, 6, 8, 0, 2, 4, 6, 8, 0}, // Nhâm
        {9, 1, 3, 5, 7, 9, 1, 3, 5, 7, 9, 1}  // Quý
    };

    /// <summary>
    /// JDN for 1984-01-01 (Lunar New Year = Giáp Tý).
    /// </summary>
    private const int JdnGiápTý = 2444235;

    /// <summary>
    /// Number of unique Can Chi values for days.
    /// </summary>
    private const int CanChiCycleLength = 60;

    /// <summary>
    /// Number of Lunar Mansions (Tú).
    /// </summary>
    private const int NhiThapBatTuCycleLength = 28;

    /// <inheritdoc />
    public CanChiYear GetCanChiNam(int lunarYear)
    {
        int canIndex = (lunarYear + 6) % 10;
        int chiIndex = (lunarYear + 8) % 12;
        return new CanChiYear(Cans[canIndex], Chis[chiIndex]);
    }

    /// <inheritdoc />
    public CanChiMonth GetCanChiThang(int lunarYear, int lunarMonth, bool isLeapMonth)
    {
        // Validate month range
        if (lunarMonth < 1 || lunarMonth > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(lunarMonth), "Lunar month must be between 1 and 12.");
        }

        // 1. Calculate year Can index (0-9)
        int yearCanIndex = (lunarYear + 6) % 10;

        // 2. Get month Can from table (month-1 because array is 0-indexed)
        int monthCanIndex = MonthCanIndexTable[yearCanIndex, lunarMonth - 1];

        // 3. Month Chi always cycles: Tý(m1), Sửu(m2), Dần(m3)...
        int monthChiIndex = (lunarMonth - 1) % 12;

        return new CanChiMonth(Cans[monthCanIndex], Chis[monthChiIndex]);
    }

    /// <inheritdoc />
    public CanChiDay GetCanChiNgay(int jdn)
    {
        // Calculate index from JDN anchor (Giáp Tý = 2444235)
        int index = (jdn - JdnGiápTý) % CanChiCycleLength;

        // Handle negative modulo
        if (index < 0) index += CanChiCycleLength;

        var (can, chi) = CanChiNgayTable[index];
        return new CanChiDay(can, chi);
    }

    /// <inheritdoc />
    public CanChiHour GetCanChiGio(int jdn, int gioBatDau)
    {
        // Day Can index (for hour Can calculation)
        int dayCanIndex = (jdn - JdnGiápTý) % 10;
        if (dayCanIndex < 0) dayCanIndex += 10;

        // gioBatDau: 23=Tý(0), 1=Sửu(1), 3=Dần(2)...
        // Each 2-hour block shares the same Can
        int gioIndex = (gioBatDau - 23) / 2;
        if (gioIndex < 0) gioIndex += 12;

        // Can for 2-hour block: 0-Tý gets 0,1, 1-Sửu gets 2,3, etc.
        int hourCanIndex = (dayCanIndex * 2 + gioIndex) % 10;

        int hourChiIndex = gioIndex; // 0=Tý, 1=Sửu, ... 11=Hợi

        return new CanChiHour(Cans[hourCanIndex], Chis[hourChiIndex]);
    }

    /// <inheritdoc />
    public int GetThapNhiTruc(DateTime solarDate)
    {
        ArgumentNullException.ThrowIfNull(solarDate);
        
        int jdn = GetJulianDayNumber(solarDate);
        var canChiNgay = GetCanChiNgay(jdn);
        
        // Find Chi index (0=Tý, 1=Sửu, ..., 11=Hợi)
        int chiIndex = Array.IndexOf(Chis, canChiNgay.Chi);
        
        // Get solar month from ISolarTermCalculator
        int solarMonth = solarTermCalculator.GetSolarMonth(solarDate);
        
        return ThapNhiTrucLookup.GetTrucIndex(chiIndex, solarMonth);
    }

    /// <inheritdoc />
    public string GetTrucName(int index)
    {
        return ThapNhiTrucLookup.GetTrucName(index);
    }

    /// <inheritdoc />
    public int GetNhiThapBatTu(int jdn)
    {
        return jdn % NhiThapBatTuCycleLength;
    }

    /// <inheritdoc />
    public int GetJulianDayNumber(DateTime solarDate)
    {
        return lunarCalculator.GetJulianDayNumber(solarDate.Year, solarDate.Month, solarDate.Day);
    }
}
