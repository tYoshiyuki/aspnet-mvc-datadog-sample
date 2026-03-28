using System;
using System.Threading;
using Datadog.Trace;

namespace AspNetMvcDatadogSample.Web.Services
{
    public class SlowSampleService
    {
        public string Execute(int delayMs)
        {
            using (var scope = Tracer.Instance.StartActive("sample.slow-service"))
            {
                scope.Span.SetTag("sample.service", "SlowSampleService");
                scope.Span.SetTag("sample.delay_ms", delayMs.ToString());
                scope.Span.SetTag("sample.simulated", "true");

                Thread.Sleep(delayMs);

                return string.Format(
                    "SlowSampleService completed. delayMs={0}, utc={1:O}",
                    delayMs,
                    DateTime.UtcNow);
            }
        }
    }
}
