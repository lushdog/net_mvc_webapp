using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K2Calendar.Classes
{
    // Source: http://www.codehosting.net/blog/BlogEngine/post/More-fiddling-with-MVC3-and-https.aspx
    // adapted from some code I found here:
    // http://stackoverflow.com/questions/1639707/asp-net-mvc-requirehttps-in-production-only

    // Add this to your MVC3 site by adding this line:
    // filters.Add(new LimitHttpsAttribute());
    // into the RegisterGlobalFilters() method in global.asax.cs.
    //
    public class LimitHttpsAttribute : IAuthorizationFilter
    {
        private static Type ssl = typeof(RequireHttpsAttribute);

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.HttpContext != null && filterContext.HttpContext.Request != null)
            {
                /*
                if (!filterContext.HttpContext.Request.IsSecureConnection ||
                    filterContext.HttpContext.Request.IsAuthenticated)
                {
                    return;
                }
                if (!RequiresSSL(filterContext))
                {
                    filterContext.Result = Unencrypted(filterContext.HttpContext.Request);
                }
                */
                
                //MM: What I want to do is force going to SSL if Action is marked RequiredSSL and to break out of SSL if not marked
                if (RequiresSSL(filterContext) && !filterContext.HttpContext.Request.IsSecureConnection)
                {
                    filterContext.Result = Encrypted(filterContext.HttpContext.Request);
                }
                else if (!RequiresSSL(filterContext) && filterContext.HttpContext.Request.IsSecureConnection)
                {
                    filterContext.Result = Unencrypted(filterContext.HttpContext.Request);
                } 
            }
        }

        private bool RequiresSSL(AuthorizationContext filterContext)
        {
            return filterContext.ActionDescriptor != null ?
                filterContext.ActionDescriptor.GetCustomAttributes(ssl, true).Length > 0
                : false;
        }

        private RedirectResult Unencrypted(HttpRequestBase request)
        {
            return new RedirectResult(request.Url.ToString().Replace("https:", "http:"));
        }

        private RedirectResult Encrypted(HttpRequestBase request)
        {
            return new RedirectResult(request.Url.ToString().Replace("http:", "https:"));
        }
    }
}