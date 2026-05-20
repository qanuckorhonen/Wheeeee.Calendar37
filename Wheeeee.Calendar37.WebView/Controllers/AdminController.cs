using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.WebView.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            if (!HomeController.GetIsAdmin(Request))
            {
                return new RedirectToActionResult("Index", "Home", null);
            }

            return View();
        }

        public static IEnumerable<IOrchestra> GetOrchestras(HttpRequest request)
        {
            var repository = (ICalenderRepository)request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));
            var person = repository.GetPersonInfo(request.Cookies[Constants.Cookies.PersonIDs]).ToArray();

            return person?
                .SelectMany(p => p.Memberships)
                .Where(m => m.Roles.Any(r => r.IsAdmin))
                .Select(m => m.Orchestra)
                .Distinct()
                .ToArray()
                ?? [];

            //var personIDs = request.Cookies[Constants.Cookies.PersonIDs];
            //if (personIDs.IsNullOrEmpty())
            //{
            //    return [];
            //}
            //else
            //{
            //    var memberships = repository
            //        .GetMembershipsByUniqueIDs(personIDs.Split(',').Select(s => Guid.Parse(s)))
            //        .Where(m => m.Roles.Any(r => r.IsAdmin))
            //        .ToArray();
            //    if (memberships.IsNullOrEmpty())
            //    {
            //        return [];
            //    }
            //    return memberships
            //        .Select(m => m.Orchestra)
            //        .Distinct()
            //        .ToArray();
            //}
        }

        public ActionResult GetEditHtml(string o)
        {
            Guid orchestraGuid = Guid.Parse(o);
            var repository = (ICalenderRepository)Request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));
            var model = repository.GetEditableOrchestra(orchestraGuid);
            return PartialView("_EditOrchestra", model);
        }
    }
}