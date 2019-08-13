using Microsoft.ApplicationInsights.DataContracts;
using RimDev.ApplicationInsights.Filters.Processors;
using Xunit;

namespace RimDev.ApplicationInsights.Filters.Tests.Processors
{
    public class RemoveHttpUrlPasswordsTelemetryTests
    {
        private readonly RemoveHttpUrlPasswordsTelemetry sut;
        private readonly TestTelemetryProcessor next;

        public RemoveHttpUrlPasswordsTelemetryTests()
        {
            next = new TestTelemetryProcessor();
            sut = new RemoveHttpUrlPasswordsTelemetry(next);
        }

        [Fact]
        public void CallsNext()
        {
            var item = new DependencyTelemetry();

            sut.Process(item);

            Assert.True(next.ProcessMethodCalled);
        }

        [Fact]
        public void RedactsUrlPassword()
        {
            var item = new DependencyTelemetry
            {
#pragma warning disable CS0618 // Type or member is obsolete
                CommandName = "https://user:passw0rd@example.com",
#pragma warning restore CS0618 // Type or member is obsolete
                Data = "https://user:passw0rd@example.com",
                Type = "Http"
            };

            sut.Process(item);

            Assert.Equal("https://user:REDACTED@example.com", item.Data);
        }

        [Fact]
        public void DoesNotModifyUrlWithoutPassword()
        {
            var item = new DependencyTelemetry
            {
#pragma warning disable CS0618 // Type or member is obsolete
                CommandName = "https://example.com",
#pragma warning restore CS0618 // Type or member is obsolete
                Data = "https://example.com",
                Type = "Http"
            };

            sut.Process(item);

            Assert.Equal("https://example.com", item.Data);
        }
    }
}
