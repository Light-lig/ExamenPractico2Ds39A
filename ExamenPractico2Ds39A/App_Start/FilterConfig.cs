using System.Web;
using System.Web.Mvc;

namespace ExamenPractico2Ds39A
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
