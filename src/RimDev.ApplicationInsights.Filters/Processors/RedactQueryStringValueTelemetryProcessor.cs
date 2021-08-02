using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace RimDev.ApplicationInsights.Filters.Processors
{
    /// <summary>Redact the value of any QueryString argument that matches
    /// the provided list of names.  Note that the ordering of QueryString
    /// arguments will change to alphabetical ordering after this
    /// processor works on the QueryString argument collection.
    /// Matching of argument names is case insensitive (see <see cref="QueryHelpers.ParseQuery"/>).
    /// </summary>
    public class RedactQueryStringValueTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly RedactQueryStringValueTelemetryProcessorOptions _options;

        public RedactQueryStringValueTelemetryProcessor(
            ITelemetryProcessor next,
            RedactQueryStringValueTelemetryProcessorOptions options
            )
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _next = next;
        }

        public void Process(ITelemetry item)
        {
            RedactTelemetryItem(item);
            _next.Process(item);
        }

        internal void RedactTelemetryItem(ITelemetry telemetryItem)
        {
            var request = telemetryItem as RequestTelemetry;
            if (request?.Url is null) return;
            if (!request.Url.IsAbsoluteUri) return;
            if (request.Url.IsAbsoluteUri && string.IsNullOrEmpty(request.Url.Query)) return;

            var uri = request.Url;
            _options.Keys = _options.Keys ?? Array.Empty<string>();

            // https://stackoverflow.com/a/43407008
            // ParseQuery() uses KeyValueAccumulator() which does case-insensitive keys
            var query = QueryHelpers.ParseQuery(uri.Query);

            // convert to KeyValuePair, and order on the Key (makes output more predictable)
            var queryArguments = query.SelectMany(x => x.Value, (col, value) =>
                new KeyValuePair<string, string>(col.Key, value))
                .OrderBy(x => x.Key)
                .ToList();

            var qb = new QueryBuilder();
            foreach (var arg in queryArguments)
            {
                qb.Add(arg.Key,
                    _options.Keys.Contains(arg.Key, StringComparer.OrdinalIgnoreCase)
                        ? _options.RedactedValue
                        : arg.Value);
            }

            var resultUri = new UriBuilder
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
                Port = uri.Port,
                Fragment = uri.Fragment,
                Path = uri.AbsolutePath,
                Query = qb.ToString()
            };
            request.Url = resultUri.Uri;
        }
    }
}
