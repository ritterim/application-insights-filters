using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace RimDev.ApplicationInsights.Filters.Tests
{
    public class TestTelemetryProcessor : ITelemetryProcessor
    {
        public bool ProcessMethodCalled;

        public void Process(ITelemetry item)
        {
            ProcessMethodCalled = true;
        }
    }
}
