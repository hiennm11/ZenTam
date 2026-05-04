namespace ZenTam.Api.Features.Calendars.Data;

public static class ThapNhiTrucLookup
{
    // Chi index: 0=Tý, 1=Sửu, 2=Dần, 3=Mão, 4=Thìn, 5=Tỵ,
    //           6=Ngọ, 7=Mùi, 8=Thân, 9=Dậu, 10=Tuất, 11=Hợi
    // Month: 1-12 (solar month from ISolarTermCalculator)
    // Returns: TrucType index 0-11

    private static readonly int[,] Table = new int[,]
    {
        // Month:  1   2   3   4   5   6   7   8   9  10  11  12
        /* Tý */ { 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11 },
        /* Sửu */ {11,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10 },
        /* Dần */ {10, 11,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9 },
        /* Mão */ { 9, 10, 11,  0,  1,  2,  3,  4,  5,  6,  7,  8 },
        /* Thìn */{ 8,  9, 10, 11,  0,  1,  2,  3,  4,  5,  6,  7 },
        /* Tỵ  */{ 7,  8,  9, 10, 11,  0,  1,  2,  3,  4,  5,  6 },
        /* Ngọ */{ 6,  7,  8,  9, 10, 11,  0,  1,  2,  3,  4,  5 },
        /* Mùi */{ 5,  6,  7,  8,  9, 10, 11,  0,  1,  2,  3,  4 },
        /* Thân */{ 4,  5,  6,  7,  8,  9, 10, 11,  0,  1,  2,  3 },
        /* Dậu */{ 3,  4,  5,  6,  7,  8,  9, 10, 11,  0,  1,  2 },
        /* Tuất */{ 2,  3,  4,  5,  6,  7,  8,  9, 10, 11,  0,  1 },
        /* Hợi */{ 1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11,  0 }
    };

    private static readonly string[] TrucNames =
    [
        "Trực Kiến", "Trực Trừ", "Trực Mãn", "Trực Bình", "Trực Định", "Trực Chấp",
        "Trực Phá", "Trực Nguy", "Trực Thành", "Trực Thu", "Trực Khai", "Trực Bế"
    ];

    public static int GetTrucIndex(int chiIndex, int solarMonth)
    {
        return Table[chiIndex, solarMonth - 1];
    }

    public static string GetTrucName(int index)
    {
        if (index < 0 || index >= 12)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Truc index must be between 0 and 11.");
        }
        return TrucNames[index];
    }
}
