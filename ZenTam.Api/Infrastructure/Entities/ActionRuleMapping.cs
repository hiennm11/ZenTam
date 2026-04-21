using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Infrastructure.Entities;

public class ActionRuleMapping
{
    public int      Id                { get; set; }
    public string   ActionId          { get; set; } = string.Empty;
    public string   RuleCode          { get; set; } = string.Empty;
    public bool     IsMandatory       { get; set; }
    public Gender?  GenderConstraint  { get; set; }
}
