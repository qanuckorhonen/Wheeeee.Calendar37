using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Wheeeee.Calendar37.Core;

namespace Wheeeee.Calendar37.WebView.Controllers
{
    public class DontKnowYouController : Controller
    {
        public IActionResult Index()
        {
            Response.Cookies.Append(Constants.Cookies.PersonIDs, "5AE048F2-9C80-496B-A80D-5F4648EFC7A1", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddYears(10)
            });

            return View();
        }
    }
}
