namespace ZenTam.Api.Common.Rules.Models;

using ZenTam.Api.Common.Domain;
using ZenTam.Api.Common.CanChi.Models;

public class RuleContext
{
    public required UserProfile Profile { get; init; }
    public required LunarDateContext Lunar { get; init; }
    public CanChiDay? CanChiNgay { get; init; }
    public CanChiYear? CanChiTuoi { get; init; }
}