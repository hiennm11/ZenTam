using ZenTam.Api.Features.Calendars.Models;

namespace ZenTam.Api.Features.Calendars.Data;

public static class HoangDaoLookup
{
    // 10 Can names in order
    private static readonly string[] CanNames =
        ["Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý"];

    // 12 Chis in order: Tý=0, Sửu=1, Dần=2, Mão=3, Thìn=4, Tỵ=5,
    //                    Ngọ=6, Mùi=7, Thân=8, Dậu=9, Tuất=10, Hợi=11
    private static readonly string[] ChiNames =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    // All 12 chi names for Hắc Đạo computation
    private static readonly string[] AllChis = ChiNames;

    // Pattern A: Can ends with Tý/Ngọ/Dậu (canIndex 0,2,7 + chi 0,6,9) → Tốt: Tý,Sửu,Ngọ,Mùi,Dậu,Hợi
    // Pattern B: Can ends with Sửu/Mùi/Thân (canIndex 1,3,6 + chi 1,7,8) → Tốt: Dần,Mão,Ngọ,Mùi,Thân,Tuất
    // Pattern C: Can ends with Dần/Tỵ/Hợi (canIndex 4,5,8,9 + chi 2,5,11) → Tốt: Tý,Sửu,Thìn,Tỵ,Dậu,Hợi
    // Pattern D: Can ends with Mão/Tuất (canIndex 1,3,6 + chi 3,10) → Tốt: Dần,Mão,Ngọ,Mùi,Thân,Tuất
    // Pattern E: Can ends with Thìn (canIndex 4 + chi 4) → Tốt: Tý,Sửu,Ngọ,Mùi,Dậu,Hợi

    private static readonly string[] PatternAHoangDaoHours = ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"];
    private static readonly string[] PatternBHoangDaoHours = ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"];
    private static readonly string[] PatternCHoangDaoHours = ["Tý", "Sửu", "Thìn", "Tỵ", "Dậu", "Hợi"];
    private static readonly string[] PatternDHoangDaoHours = ["Dần", "Mão", "Ngọ", "Mùi", "Thân", "Tuất"];
    private static readonly string[] PatternEHoangDaoHours = ["Tý", "Sửu", "Ngọ", "Mùi", "Dậu", "Hợi"];

    private static readonly string[] PatternATopHours = ["Tý", "Ngọ", "Mùi"];
    private static readonly string[] PatternBTopHours = ["Ngọ", "Mão", "Thân"];
    private static readonly string[] PatternCTopHours = ["Tý", "Thìn", "Dậu"];
    private static readonly string[] PatternDTopHours = ["Ngọ", "Mão", "Thân"];
    private static readonly string[] PatternETopHours = ["Tý", "Ngọ", "Mùi"];

    /// <summary>
    /// Determines which pattern applies based on canIndex and chiIndex.
    /// Returns 'A', 'B', 'C', 'D', 'E', or '\0' if none match.
    /// </summary>
    private static char GetPattern(int canIndex, int chiIndex)
    {
        // Pattern E: Can ends with Thìn (canIndex 4 = Mậu + chi 4 = Thìn)
        if (canIndex == 4 && chiIndex == 4)
            return 'E';

        // Pattern A: Can ends with Tý/Ngọ/Dậu (canIndex 0,2,7 + chi 0,6,9)
        if ((canIndex == 0 || canIndex == 2 || canIndex == 7) &&
            (chiIndex == 0 || chiIndex == 6 || chiIndex == 9))
            return 'A';

        // Pattern B: Can ends with Sửu/Mùi/Thân (canIndex 1,3,6 + chi 1,7,8)
        if ((canIndex == 1 || canIndex == 3 || canIndex == 6) &&
            (chiIndex == 1 || chiIndex == 7 || chiIndex == 8))
            return 'B';

        // Pattern C: Can ends with Dần/Tỵ/Hợi (canIndex 4,5,8,9 + chi 2,5,11)
        if ((canIndex == 4 || canIndex == 5 || canIndex == 8 || canIndex == 9) &&
            (chiIndex == 2 || chiIndex == 5 || chiIndex == 11))
            return 'C';

        // Pattern D: Can ends with Mão/Tuất (canIndex 1,3,6 + chi 3,10)
        if ((canIndex == 1 || canIndex == 3 || canIndex == 6) &&
            (chiIndex == 3 || chiIndex == 10))
            return 'D';

        return '\0';
    }

    /// <summary>
    /// Computes the inverse (Hắc Đạo) hours - the 6 hours not in Hoàng Đạo.
    /// </summary>
    private static string[] ComputeHacDaoHours(string[] hoangDaoHours)
    {
        return AllChis.Except(hoangDaoHours).ToArray();
    }

    public static HoangDaoInfo GetHoangDao(int canIndex, int chiIndex)
    {
        char pattern = GetPattern(canIndex, chiIndex);
        bool isHoangDao = pattern != '\0';

        if (!isHoangDao)
        {
            // Not Hoàng Đạo - return empty lists
            return new HoangDaoInfo(
                IsHoangDao: false,
                HoangDaoHours: [],
                HacDaoHours: AllChis.ToList(),
                TopHours: []
            );
        }

        string[] hoangDaoHours = pattern switch
        {
            'A' => PatternAHoangDaoHours,
            'B' => PatternBHoangDaoHours,
            'C' => PatternCHoangDaoHours,
            'D' => PatternDHoangDaoHours,
            'E' => PatternEHoangDaoHours,
            _ => throw new InvalidOperationException($"Unknown pattern: {pattern}")
        };

        string[] topHours = pattern switch
        {
            'A' => PatternATopHours,
            'B' => PatternBTopHours,
            'C' => PatternCTopHours,
            'D' => PatternDTopHours,
            'E' => PatternETopHours,
            _ => throw new InvalidOperationException($"Unknown pattern: {pattern}")
        };

        return new HoangDaoInfo(
            IsHoangDao: true,
            HoangDaoHours: hoangDaoHours.ToList(),
            HacDaoHours: ComputeHacDaoHours(hoangDaoHours).ToList(),
            TopHours: topHours.ToList()
        );
    }

    public static HoangDaoInfo GetHoangDao(string can, string chi)
    {
        int canIndex = Array.IndexOf(CanNames, can);
        int chiIndex = Array.IndexOf(ChiNames, chi);
        return GetHoangDao(canIndex, chiIndex);
    }
}
