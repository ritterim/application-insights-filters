using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.RegularExpressions;

namespace RimDev.ApplicationInsights.Filters.Processors
{
    public class RemoveHttpUrlPasswordsTelemetry : ITelemetryProcessor
    {
        private static readonly Regex removePasswordRegex =
            new Regex(@"http(s)?:\/\/.+:(?<password>.+)@", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ITelemetryProcessor next;

        public RemoveHttpUrlPasswordsTelemetry(ITelemetryProcessor next)
        {
            this.next = next;
        }

        public void Process(ITelemetry item)
        {
            var request = item as DependencyTelemetry;

            if (request != null && request.Type == "Http")
            {
#pragma warning disable CS0618 // Type or member is obsolete
                request.CommandName = RemovePasswordFromUrl(request.CommandName);
#pragma warning restore CS0618 // Type or member is obsolete
                request.Data = RemovePasswordFromUrl(request.Data);
            }

            next.Process(item);
        }

        private static string RemovePasswordFromUrl(string url)
        {
            if (url == null)
            {
                return null;
            }

            var match = removePasswordRegex.Match(url).Groups["password"];

            if (match.Success)
            {
                url = url.Replace(match.Value, "REDACTED");
            }

            return url;
        }
    }
}
