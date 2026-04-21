using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Infrastructure.Entities;

public class User
{
    public Guid     Id        { get; set; }
    public string   Username  { get; set; } = string.Empty;
    public Gender   Gender    { get; set; }
    public DateTime SolarDOB  { get; set; }
    public int      LunarYOB  { get; set; }
}
