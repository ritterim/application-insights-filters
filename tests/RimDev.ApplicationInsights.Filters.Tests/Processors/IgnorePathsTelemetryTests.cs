using Microsoft.ApplicationInsights.DataContracts;
using RimDev.ApplicationInsights.Filters.Processors;
using System;
using Xunit;

namespace RimDev.ApplicationInsights.Filters.Tests.Processors
{
    public class IgnorePathsTelemetryTests
    {
        private readonly IgnorePathsTelemetry sut;
        private readonly TestTelemetryProcessor next;

        public IgnorePathsTelemetryTests()
        {
            next = new TestTelemetryProcessor();
            sut = new IgnorePathsTelemetry(next, "/ignore");
        }

        [Fact]
        public void CallsNextForNonMatchingUrl()
        {
            var item = new RequestTelemetry
            {
                Url = new Uri("https://example.org/abc")
            };

            sut.Process(item);

            Assert.True(next.ProcessMethodCalled);
        }

        [Fact]
        public void CallsNextForNullUrl()
        {
            var item = new RequestTelemetry
            {
                Url = null
            };

            sut.Process(item);

            Assert.True(next.ProcessMethodCalled);
        }

        [Fact]
        public void DoesNotCallNextForMatchingUrl()
        {
            var item = new RequestTelemetry
            {
                Url = new Uri("https://example.org/ignore")
            };

            sut.Process(item);

            Assert.False(next.ProcessMethodCalled);
        }
    }
}
