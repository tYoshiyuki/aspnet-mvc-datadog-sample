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

        public void ExecuteAndThrow(int delayMs)
        {
            using (var scope = Tracer.Instance.StartActive("sample.exception-service"))
            {
                scope.Span.SetTag("sample.service", "SlowSampleService");
                scope.Span.SetTag("sample.delay_ms", delayMs.ToString());
                scope.Span.SetTag("sample.simulated", "true");
                scope.Span.SetTag("sample.exception_test", "true");

                try
                {
                    Thread.Sleep(delayMs);

                    throw new InvalidOperationException(
                        string.Format("Datadog exception test. delayMs={0}", delayMs));
                }
                catch (Exception ex)
                {
                    scope.Span.SetTag("error", "true");
                    scope.Span.SetTag("error.msg", ex.Message);
                    scope.Span.SetTag("error.type", ex.GetType().FullName);

                    throw;
                }
            }
        }
    }
}
