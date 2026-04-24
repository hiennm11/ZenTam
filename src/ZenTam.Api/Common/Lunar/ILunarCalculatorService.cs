using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Common.Lunar;

public interface ILunarCalculatorService
{
    /// <summary>Returns the lunar year corresponding to the given solar date.</summary>
    int GetLunarYear(DateTime solarDate);

    /// <summary>Converts a solar date to a full <see cref="LunarDateContext"/>.</summary>
    LunarDateContext Convert(DateTime solarDate);
}
