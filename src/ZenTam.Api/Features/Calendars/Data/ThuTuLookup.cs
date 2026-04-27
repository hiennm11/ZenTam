namespace ZenTam.Api.Features.Calendars.Data;

public static class ThuTuLookup
{
    // 12 Chis in order: Tý=0, Sửu=1, Dần=2, Mão=3, Thìn=4, Tỵ=5,
    //                    Ngọ=6, Mùi=7, Thân=8, Dậu=9, Tuất=10, Hợi=11
    private static readonly string[] ChiNames =
        ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];

    // lunarMonth 1-12 → array of 2 forbidden chi indices
    private static readonly int[][] Table =
    [
        /*  1  */ [0, 6],   // Tý, Ngọ
        /*  2  */ [1, 7],   // Sửu, Mùi
        /*  3  */ [2, 8],   // Dần, Thân
        /*  4  */ [3, 9],   // Mão, Dậu
        /*  5  */ [4, 10],  // Thìn, Tuất
        /*  6  */ [5, 11],  // Tỵ, Hợi
        /*  7  */ [6, 0],   // Ngọ, Tý
        /*  8  */ [7, 1],   // Mùi, Sửu
        /*  9  */ [8, 2],   // Thân, Dần
        /* 10  */ [9, 3],   // Dậu, Mão
        /* 11  */ [10, 4],  // Tuất, Thìn
        /* 12  */ [11, 5]   // Hợi, Tỵ
    ];

    public static bool IsThuTu(int lunarMonth, int chiIndex)
    {
        var forbidden = Table[lunarMonth - 1];
        return forbidden[0] == chiIndex || forbidden[1] == chiIndex;
    }

    public static int[] GetForbiddenChi(int lunarMonth)
        => Table[lunarMonth - 1];

    public static string[] GetForbiddenChiNames(int lunarMonth)
    {
        var indices = Table[lunarMonth - 1];
        return [ChiNames[indices[0]], ChiNames[indices[1]]];
    }
}
