using System.Text.RegularExpressions;

namespace ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;

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
