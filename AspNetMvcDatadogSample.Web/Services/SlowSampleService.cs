using System;
using System.Text;
using System.Threading;
using Datadog.Trace;

namespace AspNetMvcDatadogSample.Web.Services
{
    public class SlowSampleService
    {
        private static readonly object SharedLock = new object();

        public string Execute(int delayMs)
        {
            using (var scope = Tracer.Instance.StartActive("sample.slow-service"))
            {
                scope.Span.SetTag("sample.service", "SlowSampleService");
                scope.Span.SetTag("sample.delay_ms", delayMs.ToString());
                scope.Span.SetTag("sample.simulated", "true");

                var result = SimulateLogicalDelay(delayMs);

                return string.Format(
                    "SlowSampleService completed. delayMs={0}, result={1}, utc={2:O}",
                    delayMs,
                    result,
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
                    SimulateLogicalDelay(delayMs);

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

        private static string SimulateLogicalDelay(int delayMs)
        {
            var contentionMs = Math.Max(50, delayMs / 5);
            var cpuIterations = Math.Max(10000, delayMs * 400);
            var allocationLoops = Math.Max(5, delayMs / 100);

            SimulateLockContention(contentionMs);
            var cpuScore = SimulateCpuWork(cpuIterations);
            var textLength = SimulateAllocationWork(allocationLoops);

            return string.Format(
                "contentionMs={0}, cpuIterations={1}, textLength={2}, cpuScore={3:F2}",
                contentionMs,
                cpuIterations,
                textLength,
                cpuScore);
        }

        private static void SimulateLockContention(int holdMs)
        {
            using (Tracer.Instance.StartActive("sample.lock-contention"))
            using (var ready = new ManualResetEventSlim(false))
            {
                var worker = new Thread(() =>
                {
                    lock (SharedLock)
                    {
                        ready.Set();
                        Thread.Sleep(holdMs);
                    }
                });

                worker.IsBackground = true;
                worker.Start();

                ready.Wait();

                lock (SharedLock)
                {
                }

                worker.Join();
            }
        }

        private static double SimulateCpuWork(int iterations)
        {
            using (Tracer.Instance.StartActive("sample.cpu-work"))
            {
                double total = 0;

                for (var i = 1; i <= iterations; i++)
                {
                    total += Math.Sqrt(i) * Math.Sin(i * 0.01d);
                }

                return total;
            }
        }

        private static int SimulateAllocationWork(int loops)
        {
            using (Tracer.Instance.StartActive("sample.allocation-work"))
            {
                var builder = new StringBuilder();

                for (var i = 0; i < loops; i++)
                {
                    builder.Append(i);
                    builder.Append(':');

                    for (var j = 0; j < 12; j++)
                    {
                        builder.Append(Guid.NewGuid().ToString("N"));
                    }

                    builder.AppendLine();
                }

                var text = builder.ToString();
                var normalized = text.ToUpperInvariant().Replace("A", "a");

                return normalized.Length;
            }
        }
    }
}
