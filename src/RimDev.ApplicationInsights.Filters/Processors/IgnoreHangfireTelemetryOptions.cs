namespace RimDev.ApplicationInsights.Filters
{
    public class IgnoreHangfireTelemetryOptions
    {
        public string SqlConnectionString { get; set; }

        public string HangfireDashboardPath { get; set; } = "/hangfire";
    }
}
