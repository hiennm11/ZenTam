using System.Text.RegularExpressions;

namespace ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;

public class RegexIntentParser : IIntentParser
{
    private static readonly (string ActionCode, Regex Pattern)[] ActionPatterns =
    [
        // ========== GIA DINH ==========
        ("XAY_NHA",   new Regex(@"động thổ|cất nóc|cất nhà|xây nhà|làm nhà|dựng nhà",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("SUA_NHA",   new Regex(@"sửa nhà|sua nhà|tu tạo nhà",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("NHAP_TRACH", new Regex(@"nhập trạch|vào nhà mới|chuyển nhà",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("CUOI_HOI",  new Regex(@"cưới|lấy vợ|lấy chồng|hỏi vợ|kết hôn",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("SINH_CON",  new Regex(@"sinh con|đẻ con|có em bé",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== NGHIEP_TAI ==========
        ("KHAI_TRUONG", new Regex(@"khai trương|mở cửa hàng|mở công ty",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("KY_HOP_DONG", new Regex(@"ký hợp đồng|ký hợp đồng lớn|ký kết giao dịch",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("NHAN_VIEC",   new Regex(@"nhận việc|nhậm chức|thăng chức|chuyển việc",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("MUA_VANG",    new Regex(@"mua vàng|mua vang|tậu vàng",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("MUA_DAT",     new Regex(@"mua đất|mua bất động sản",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("MUA_XE",      new Regex(@"mua xe|tậu xe|tậu ô tô|mua ô tô",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("DAM_BAO_HANH", new Regex(@"bảo hành|ký bảo lãnh",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== DI_CHUYEN ==========
        ("XUAT_HANH", new Regex(@"xuất hành|khởi hành|đi xa|du học",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("CU_HUONG",  new Regex(@"về quê|cứ hương|thăm viếng tổ tiên|viếng tổ tiên",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("BAT_DAU",   new Regex(@"bắt đầu hành trình|khởi sự",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== SUC_KHOE ==========
        ("CHUA_BENH", new Regex(@"chữa bệnh|khám bệnh|đi bệnh viện",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("TAM_SOAT",  new Regex(@"tầm soát|kiểm tra sức khỏe",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== HOC_TAP ==========
        ("KHAI_VONG", new Regex(@"khai giảng|năm học mới",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("THI_DAU",   new Regex(@"thi cử|tham gia cuộc thi",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== AM_PHAN ==========
        ("AN_TANG",   new Regex(@"an táng|chôn cất|tang lễ",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("BOC_MO",    new Regex(@"bốc mộ|sang cát|tu tạo mộ",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("THO_MAU",   new Regex(@"thổ mộ|tìm đất mộ|tìm đất đặt mộ|tìm mộ đất",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== TAM_LINH ==========
        ("LE_BAI",    new Regex(@"lễ bái|tảo mộ|cầu an",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("CAT_SAC",   new Regex(@"cắt sắc|hoá giải|tẩy uế",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),

        // ========== KHAC ==========
        ("TU_TUC",    new Regex(@"thiền định|tu tâm|tự tứ",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
    ];

    private static readonly Regex YearNamNay  = new(@"năm nay|năm này",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex YearSangNam = new(@"sang năm|năm tới|năm sau",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex YearNam4    = new(@"năm (\d{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Year4       = new(@"(\d{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Task<List<ParsedIntent>?> TryParseAsync(string text, int currentYear, CancellationToken ct = default)
    {
        var matchedCodes = ActionPatterns
            .Where(ap => ap.Pattern.IsMatch(text))
            .Select(ap => ap.ActionCode)
            .ToList();

        if (matchedCodes.Count == 0)
            return Task.FromResult<List<ParsedIntent>?>(null);

        int targetYear;
        if (YearNamNay.IsMatch(text))
        {
            targetYear = currentYear;
        }
        else if (YearSangNam.IsMatch(text))
        {
            targetYear = currentYear + 1;
        }
        else
        {
            var m = YearNam4.Match(text);
            if (m.Success)
                targetYear = int.Parse(m.Groups[1].Value);
            else
            {
                m = Year4.Match(text);
                targetYear = m.Success ? int.Parse(m.Groups[1].Value) : currentYear;
            }
        }

        var result = matchedCodes
            .Select(code => new ParsedIntent(code, targetYear, "REGEX"))
            .ToList();

        return Task.FromResult<List<ParsedIntent>?>(result);
    }
}
