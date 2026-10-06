namespace PairOfEmployees.Server.Tests;

public sealed class FixedTimeProvider : TimeProvider
{
    public override DateTimeOffset GetUtcNow() 
        => new(2024, 2, 29, 12, 0, 0, TimeSpan.Zero);
    
    public override TimeZoneInfo LocalTimeZone 
        => TimeZoneInfo.Utc;
}
