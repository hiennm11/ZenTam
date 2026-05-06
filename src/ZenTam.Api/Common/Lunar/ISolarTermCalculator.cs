using ZenTam.Api.Common.Lunar.Models;

namespace ZenTam.Api.Common.Lunar;

public interface ISolarTermCalculator
{
    IReadOnlyList<SolarTermResult> GetSolarTerms(int year);
    SolarTermResult GetSolarTerm(string termName, int year);
    double GetSunLongitude(double jdn);
    double GetTrueLongitude(double jdn);
}