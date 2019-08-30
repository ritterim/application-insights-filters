using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Data.SqlClient;

namespace RimDev.ApplicationInsights.Filters.Processors
{
    /// <summary>
    /// Ignore the dashboard and Hangfire SQL backend telemetry.
    /// This does not support ignoring Redis or any other backend.
    /// </summary>
    public class IgnoreHangfireTelemetry : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        private readonly string hangfireDashboardPath;
        private readonly string sqlDatabase;

        public IgnoreHangfireTelemetry(
            ITelemetryProcessor next,
            IgnoreHangfireTelemetryOptions options)
            : this(next, options.SqlConnectionString, options.HangfireDashboardPath)
        { }

        public IgnoreHangfireTelemetry(
            ITelemetryProcessor next,
            string sqlConnectionString = null,
            string hangfireDashboardPath = "/hangfire")
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));

            if (!string.IsNullOrEmpty(sqlConnectionString))
            {
                var builder = new SqlConnectionStringBuilder(sqlConnectionString);

                sqlDatabase = builder.InitialCatalog;
            }

            this.hangfireDashboardPath = hangfireDashboardPath ?? throw new ArgumentNullException(nameof(hangfireDashboardPath));
        }

        public void Process(ITelemetry item)
        {
            var request = item as RequestTelemetry;

            if (request != null
                && request.Url.AbsolutePath.StartsWith(hangfireDashboardPath))
            {
                return;
            }

            if (sqlDatabase != null)
            {
                var sqlBackend = item as DependencyTelemetry;

                if (sqlBackend != null
                    && sqlBackend.Type == "SQL"
                    && sqlBackend.Target.EndsWith($"| {sqlDatabase}", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            next.Process(item);
        }
    }
}
