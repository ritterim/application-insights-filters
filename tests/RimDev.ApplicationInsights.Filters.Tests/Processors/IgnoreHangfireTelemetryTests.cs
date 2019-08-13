using Microsoft.ApplicationInsights.DataContracts;
using RimDev.ApplicationInsights.Filters.Processors;
using System;
using Xunit;

namespace RimDev.ApplicationInsights.Filters.Tests.Processors
{
    public class IgnoreHangfireTelemetryTests
    {
        private readonly IgnoreHangfireTelemetry sut;
        private readonly TestTelemetryProcessor next;

        public IgnoreHangfireTelemetryTests()
        {
            next = new TestTelemetryProcessor();
            sut = new IgnoreHangfireTelemetry(
                next,
                "Server=Test;Database=Hangfire;Trusted_Connection=True;");
        }

        [Fact]
        public void CallsNextForNonHangfireDashboardUrl()
        {
            var item = new RequestTelemetry
            {
                Url = new Uri("https://example.org/abc")
            };

            sut.Process(item);

            Assert.True(next.ProcessMethodCalled);
        }

        [Fact]
        public void DoesNotCallNextForHangfireDashboard()
        {
            var item = new RequestTelemetry
            {
                Url = new Uri("https://example.org/hangfire")
            };

            sut.Process(item);

            Assert.False(next.ProcessMethodCalled);
        }

        [Fact]
        public void CallsNextForNonHangfireSql()
        {
            var item = new DependencyTelemetry
            {
                Type = "SQL",
                Target = " | SomeDatabase"
            };

            sut.Process(item);

            Assert.True(next.ProcessMethodCalled);
        }

        [Fact]
        public void DoesNotCallNextForHangfireSql()
        {
            var item = new DependencyTelemetry
            {
                Type = "SQL",
                Target = " | Hangfire"
            };

            sut.Process(item);

            Assert.False(next.ProcessMethodCalled);
        }
    }
}
