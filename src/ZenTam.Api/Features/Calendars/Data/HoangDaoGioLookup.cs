namespace ZenTam.Api.Features.Calendars.Data;

/// <summary>
/// Giờ Hoàng Đạo lookup based on Can of the day.
/// Each Can (10 Heavenly Stems) maps to 6 good Chi hours.
/// </summary>
public static class HoangDaoGioLookup
{
    // Can names in order: 0=Giáp, 1=Ất, 2=Bính, 3=Đinh, 4=Mậu, 5=Kỷ, 6=Canh, 7=Tân, 8=Nhâm, 9=Quý
    private static readonly string[] CanNames =
        ["Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý"];

    // 12 Chi names: Tý=0, Sửu=1, Dần=2, Mão=3, Thìn=4, Tỵ=5, Ngọ=6, Mùi=7, Thân=8, Dậu=9, Tuất=10, Hợi=11
    private static readonly string[] ChiNames =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    // Table: index 0-9 maps to array of 6 good hours (Chi names)
    private static readonly string[][] Table =
    [
        // Index 0: Giáp → Tý, Sửu, Ngọ, Mùi, Dậu, Hợi
        ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
        // Index 1: Ất → Dần, Mão, Ngọ, Mùi, Thân, Tuất
        ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"],
        // Index 2: Bính → Tý, Sửu, Thìn, Tỵ, Dậu, Hợi
        ["Tý", "Sửu", "Thìn", "Tỵ", "Dậu", "Hợi"],
        // Index 3: Đinh → Dần, Mão, Ngọ, Mùi, Thân, Tuất
        ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"],
        // Index 4: Mậu → Tý, Sửu, Ngọ, Mùi, Dậu, Hợi
        ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
        // Index 5: Kỷ → Dần, Mão, Ngọ, Mùi, Thân, Tuất
        ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"],
        // Index 6: Canh → Dần, Mão, Ngọ, Mùi, Thân, Tuất
        ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"],
        // Index 7: Tân → Tý, Sửu, Ngọ, Mùi, Dậu, Hợi
        ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"],
        // Index 8: Nhâm → Tý, Sửu, Thìn, Tỵ, Dậu, Hợi
        ["Tý", "Sửu", "Thìn", "Tỵ", "Dậu", "Hợi"],
        // Index 9: Quý → Dần, Mão, Ngọ, Mùi, Thân, Tuất
        ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"]
    ];

    /// <summary>
    /// Gets the 6 Hoàng Đạo hours for a given Can index (0-9).
    /// </summary>
    public static string[] GetHoangGio(int canIndex) => Table[canIndex];

    /// <summary>
    /// Gets the 6 Hoàng Đạo hours for a given Can name.
    /// </summary>
    public static string[] GetHoangGio(string can)
    {
        int canIndex = Array.IndexOf(CanNames, can);
        if (canIndex < 0)
            throw new ArgumentException($"Invalid Can name: {can}", nameof(can));
        return Table[canIndex];
    }

    /// <summary>
    /// Gets the Hoàng Đạo hours for a given Can index and returns them as a formatted string.
    /// </summary>
    public static string GetHoangGioDisplay(int canIndex)
        => string.Join(", ", Table[canIndex]);
}