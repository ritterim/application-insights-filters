# Application Insights Filters

Filters for Microsoft Application Insights ASP.NET and ASP.NET Core applications.

## Installation

Install the [RimDev.ApplicationInsights.Filters][NuGet link] NuGet package.

```
PM> Install-Package RimDev.ApplicationInsights.Filters
```

## Usage

Follow the usage guidelines for ASP.NET and ASP.NET Core at [https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling](https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling).

```csharp
namespace WebApplication
{
    public class Startup
    {
        // ASP.NET Full Framework
        public void Configuration(IAppBuilder app)
        {
            var telemetryProcessorChainBuilder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;

            telemetryProcessorChainBuilder
                .Use(next => new IgnoreHangfireTelemetry(next,
                    ConfigurationManager.ConnectionStrings["Hangfire"]?.ConnectionString))
                .Use(next => new IgnorePathsTelemetry(next, "/_health"))
                .Use(next => new RemoveHttpUrlPasswordsTelemetry(next))
                .Build();
        }

        // ASP.NET Core
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new IgnoreHangfireTelemetryOptions
            {
                SqlConnectionString = configuration.GetConnectionString("hangfire")
            });
            services.AddSingleton(new IgnorePathsTelemetryOptions
            {
                Paths = new[] { "/_admin" }
            });

            services.AddApplicationInsightsTelemetry();
            services.AddApplicationInsightsTelemetryProcessor<IgnoreHangfireTelemetry>();
            services.AddApplicationInsightsTelemetryProcessor<IgnorePathsTelemetry>();
            services.AddApplicationInsightsTelemetryProcessor<RemoveHttpUrlPasswordsTelemetry>();
        }
    }
}
```

## License

MIT License

[NuGet link]: https://www.nuget.org/packages/RimDev.ApplicationInsights.Filters
