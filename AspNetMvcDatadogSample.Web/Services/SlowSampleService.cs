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
            var span = Tracer.Instance.ActiveScope?.Span;
            span?.SetTag("sample.service", "SlowSampleService");
            span?.SetTag("sample.delay_ms", delayMs.ToString());
            span?.SetTag("sample.simulated", "true");

            var result = SimulateLogicalDelay(delayMs);

            return string.Format(
                "SlowSampleService completed. delayMs={0}, result={1}, utc={2:O}",
                delayMs,
                result,
                DateTime.UtcNow);
        }

        public void ExecuteAndThrow(int delayMs)
        {
            var span = Tracer.Instance.ActiveScope?.Span;
            span?.SetTag("sample.service", "SlowSampleService");
            span?.SetTag("sample.delay_ms", delayMs.ToString());
            span?.SetTag("sample.simulated", "true");
            span?.SetTag("sample.exception_test", "true");

            try
            {
                SimulateLogicalDelay(delayMs);

                throw new ApplicationException(
                    string.Format("Datadog exception test. delayMs={0}, {1}", delayMs, DateTime.Now));
            }
            catch (Exception ex)
            {
                span?.SetTag("error", "true");
                span?.SetTag("error.msg", ex.Message);
                span?.SetTag("error.type", ex.GetType().FullName);

                throw;
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
            double total = 0;

            for (var i = 1; i <= iterations; i++)
            {
                total += Math.Sqrt(i) * Math.Sin(i * 0.01d);
            }

            return total;
        }

        private static int SimulateAllocationWork(int loops)
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
