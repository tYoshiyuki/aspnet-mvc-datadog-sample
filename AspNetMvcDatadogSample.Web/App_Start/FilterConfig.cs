using System.Web;
using System.Web.Mvc;

namespace AspNetMvcDatadogSample.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
