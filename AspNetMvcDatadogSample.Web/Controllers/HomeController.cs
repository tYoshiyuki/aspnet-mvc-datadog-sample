using System;
using System.Web.Mvc;
using AspNetMvcDatadogSample.Web.Services;
using Datadog.Trace;

namespace AspNetMvcDatadogSample.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly SlowSampleService _slowSampleService = new SlowSampleService();

        public ActionResult Index()
        {
            SetDatadogMetadata("home.index", 0);

            return View();
        }

        public ActionResult About()
        {
            SetDatadogMetadata("home.about", 0);
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            SetDatadogMetadata("home.contact", 0);
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Slow(int delayMs = 3000)
        {
            delayMs = NormalizeDelayMs(delayMs);

            SetDatadogMetadata("home.slow", delayMs);

            ViewBag.Message = "時間のかかるサービスを呼び出しました。";
            ViewBag.DelayMs = delayMs;
            ViewBag.Result = _slowSampleService.Execute(delayMs);

            return View();
        }

        public ActionResult ExceptionTest(int delayMs = 1000)
        {
            delayMs = NormalizeDelayMs(delayMs);

            SetDatadogMetadata("home.exception", delayMs);
            _slowSampleService.ExecuteAndThrow(delayMs);

            return new EmptyResult();
        }

        private static int NormalizeDelayMs(int delayMs)
        {
            return Math.Max(0, Math.Min(delayMs, 30000));
        }

        private void SetDatadogMetadata(string operationName, int delayMs)
        {
            var activeScope = Tracer.Instance.ActiveScope;
            if (activeScope == null)
            {
                return;
            }

            activeScope.Span.SetTag("sample.operation", operationName);
            activeScope.Span.SetTag("sample.controller", RouteData.Values["controller"] as string ?? "unknown");
            activeScope.Span.SetTag("sample.action", RouteData.Values["action"] as string ?? "unknown");
            activeScope.Span.SetTag("sample.delay_ms", delayMs.ToString());
            activeScope.Span.SetTag("sample.machine_name", Environment.MachineName);
            activeScope.Span.SetTag("sample.request_id", Guid.NewGuid().ToString("N"));
        }
    }
}