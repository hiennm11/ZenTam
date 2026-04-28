using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Services;

public class ActionCodeMapper
{
    private readonly ZenTamDbContext _db;
    private readonly Dictionary<string, ActionCode> _stringToEnumCache;
    private readonly Dictionary<ActionCode, string> _enumToStringCache;

    public ActionCodeMapper(ZenTamDbContext db)
    {
        _db = db;
        _stringToEnumCache = new Dictionary<string, ActionCode>(StringComparer.OrdinalIgnoreCase);
        _enumToStringCache = new Dictionary<ActionCode, string>();

        // Initialize bidirectional mapping from enum
        foreach (ActionCode code in Enum.GetValues<ActionCode>())
        {
            if (code == ActionCode.UNKNOWN) continue;
            string key = code.ToString(); // e.g., "XAY_NHA"
            _stringToEnumCache[key] = code;
            _enumToStringCache[code] = key;
        }
    }

    /// <summary>
    /// Get rule codes for an action, filtered by gender scope and rule tier.
    /// </summary>
    /// <param name="actionId">The action identifier (e.g., "XAY_NHA")</param>
    /// <param name="userGender">The user's gender for scope filtering</param>
    /// <param name="requestedTier">The rule tier to filter by (Year or Day)</param>
    /// <returns>Ordered list of rule codes that apply</returns>
    public IReadOnlyList<string> GetRulesForAction(string actionId, Gender userGender, RuleTier requestedTier)
    {
        return _db.ActionRuleMappings
            .AsNoTracking()
            .Where(m => m.ActionId == actionId)
            .Where(m => AppliesToUser(m.GenderScope, userGender))
            .Where(m => m.Tier == requestedTier || m.Tier == RuleTier.All)
            .OrderBy(m => m.Priority)
            .Select(m => m.RuleCode)
            .ToList();
    }

    /// <summary>
    /// Get full ActionRuleMapping entities for an action, filtered by gender scope and rule tier.
    /// </summary>
    public IReadOnlyList<ActionRuleMapping> GetRuleMappingsForAction(string actionId, Gender userGender, RuleTier requestedTier)
    {
        return _db.ActionRuleMappings
            .AsNoTracking()
            .Where(m => m.ActionId == actionId)
            .Where(m => AppliesToUser(m.GenderScope, userGender))
            .Where(m => m.Tier == requestedTier || m.Tier == RuleTier.All)
            .OrderBy(m => m.Priority)
            .ToList();
    }

    /// <summary>
    /// Determines if a mapping applies to a user based on GenderScope.
    /// </summary>
    private static bool AppliesToUser(GenderApplyScope scope, Gender userGender)
    {
        return scope == GenderApplyScope.Both
            || (scope == GenderApplyScope.MaleOnly && userGender == Gender.Male)
            || (scope == GenderApplyScope.FemaleOnly && userGender == Gender.Female);
    }

    /// <summary>
    /// Fast path: Convert enum to string (in-memory, no DB query).
    /// </summary>
    public string ToString(ActionCode code)
    {
        if (_enumToStringCache.TryGetValue(code, out var result))
            return result;
        
        return ActionCode.UNKNOWN.ToString(); // fallback
    }

    /// <summary>
    /// Convert string to enum. Falls back to UNKNOWN if not found.
    /// </summary>
    public ActionCode ToEnum(string actionId)
    {
        if (string.IsNullOrWhiteSpace(actionId))
            return ActionCode.UNKNOWN;

        // Fast path: in-memory cache
        if (_stringToEnumCache.TryGetValue(actionId, out var cached))
            return cached;

        // Slow path: DB lookup (for actions added dynamically to DB)
        var dbEntry = _db.ActionCatalog
            .AsNoTracking()
            .FirstOrDefault(ac => ac.Id == actionId);

        if (dbEntry != null)
        {
            if (Enum.TryParse<ActionCode>(actionId, ignoreCase: true, out var parsed))
            {
                _stringToEnumCache[actionId] = parsed;
                return parsed;
            }
        }

        return ActionCode.UNKNOWN;
    }

    /// <summary>
    /// Validates that all DB ActionCatalog entries have corresponding enum values.
    /// Returns list of missing enum entries.
    /// </summary>
    public IReadOnlyList<string> GetMissingEnumMappings()
    {
        var dbIds = _db.ActionCatalog
            .AsNoTracking()
            .Select(ac => ac.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = new List<string>();
        foreach (var id in dbIds)
        {
            if (!Enum.TryParse<ActionCode>(id, ignoreCase: true, out _))
            {
                missing.Add(id);
            }
        }

        return missing;
    }

    /// <summary>
    /// Validates that all enum values (except UNKNOWN) exist in DB.
    /// Returns list of enum codes missing from DB.
    /// </summary>
    public IReadOnlyList<ActionCode> GetMissingDbEntries()
    {
        var dbIds = _db.ActionCatalog
            .AsNoTracking()
            .Select(ac => ac.Id.ToUpperInvariant())
            .ToHashSet();

        var missing = new List<ActionCode>();
        foreach (ActionCode code in Enum.GetValues<ActionCode>())
        {
            if (code == ActionCode.UNKNOWN) continue;
            if (!dbIds.Contains(code.ToString()))
            {
                missing.Add(code);
            }
        }

        return missing;
    }
}