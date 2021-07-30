using Microsoft.AspNetCore.WebUtilities;

namespace RimDev.ApplicationInsights.Filters.Processors
{
    public class RedactQueryStringValueTelemetryProcessorOptions
    {
        /// <summary>What should we replace the QueryString argument value with?</summary>
        public string RedactedValue { get; set; } = "REDACTED";

        /// <summary>List of QueryString argument names to redact.  Note that in .NET Core,
        /// those argument names are case insensitive. (See <see cref="QueryHelpers.ParseQuery"/>.)
        /// </summary>
        public string[] Keys { get; set; }
    }
}
