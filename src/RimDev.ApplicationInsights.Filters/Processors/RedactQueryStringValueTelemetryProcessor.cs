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
    /// Operates on <see cref="RequestTelemetry"/> and <see cref="DependencyTelemetry"/>
    /// unless the related bool properties in <see cref="RedactQueryStringValueTelemetryProcessorOptions"/>
    /// are set to 'false'.
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
            if (_options?.RedactDependencyTelemetry == true) RedactDependencyTelemetryItem(item);
            if (_options?.RedactRequestTelemetry == true) RedactRequestTelemetryItem(item);
            _next.Process(item);
        }

        private Uri RedactUri(Uri uri)
        {
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

            return resultUri.Uri;
        }

        internal void RedactDependencyTelemetryItem(ITelemetry telemetryItem)
        {
            var request = telemetryItem as DependencyTelemetry;
            if (string.IsNullOrEmpty(request?.Data)) return;

            // This one properly treats an absolute path, but relative URI such as
            // "/something/something/dark?side=true" as "not Absolute"
            // This is the definition of "!IsAbsolute" that we want.
            if (!Uri.IsWellFormedUriString(request.Data, UriKind.Absolute)) return;

            // This one converts "/something/something/dark?side=true" into
            // "file://something/something/dark?side=true" for some reason.
            // But we still use it because we want to safely create the Uri object.
            if (!Uri.TryCreate(request.Data, UriKind.Absolute, out var uri)) return;

            request.Data = RedactUri(uri).ToString();
        }

        internal void RedactRequestTelemetryItem(ITelemetry telemetryItem)
        {
            var request = telemetryItem as RequestTelemetry;
            if (request?.Url is null) return;
            if (!request.Url.IsAbsoluteUri) return;
            if (request.Url.IsAbsoluteUri && string.IsNullOrEmpty(request.Url.Query)) return;

            request.Url = RedactUri(request.Url);
        }
    }
}
