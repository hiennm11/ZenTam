using System.Text.RegularExpressions;

namespace ZenTam.Api.Features.ParseAndEvaluate.IntentParsing;

public class RegexIntentParser : IIntentParser
{
    private static readonly (string ActionCode, Regex Pattern)[] ActionPatterns =
    [
        ("XAY_NHA",   new Regex(@"động thổ|cất nóc|cất nhà|xây nhà|làm nhà|dựng nhà",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("CUOI_HOI",  new Regex(@"cưới|lấy vợ|lấy chồng|hỏi vợ|kết hôn",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("MUA_XE",    new Regex(@"mua xe|tậu xe",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)),
        ("XUAT_HANH", new Regex(@"xuất hành|khởi hành|đi xa",
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

    public Task<ParsedIntent?> TryParseAsync(string text, int currentYear, CancellationToken ct = default)
    {
        string? actionCode = null;
        foreach (var (code, pattern) in ActionPatterns)
        {
            if (pattern.IsMatch(text))
            {
                actionCode = code;
                break;
            }
        }

        if (actionCode is null)
            return Task.FromResult<ParsedIntent?>(null);

        int? targetYear = null;
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
                if (m.Success)
                    targetYear = int.Parse(m.Groups[1].Value);
            }
        }

        targetYear ??= currentYear;

        return Task.FromResult<ParsedIntent?>(new ParsedIntent(actionCode, targetYear, "REGEX"));
    }
}
