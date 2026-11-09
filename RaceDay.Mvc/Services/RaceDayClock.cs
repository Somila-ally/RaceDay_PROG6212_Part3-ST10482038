namespace RaceDay.Mvc.Services;

public static class RaceDayClock
{
    private static TimeZoneInfo SouthAfrica { get; } = FindTimeZone();
    private static TimeZoneInfo FindTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time"); }
    }
    public static DateTime ToUtc(DateTime southAfricanTime) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(southAfricanTime, DateTimeKind.Unspecified), SouthAfrica);
    public static DateTime ToSouthAfricanTime(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), SouthAfrica);
}
