using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Linq;

namespace RimDev.ApplicationInsights.Filters.Processors
{
    public class IgnorePathsTelemetry : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        private readonly string[] paths;

        public IgnorePathsTelemetry(ITelemetryProcessor next, params string[] paths)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public void Process(ITelemetry item)
        {
            var request = item as RequestTelemetry;

            if (request != null && paths.Any(x => x == request.Url.AbsolutePath))
            {
                return;
            }

            next.Process(item);
        }
    }
}
