using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Core.Extensions;
using Wheeeee.Core.Interfaces.Collections;
using Wheeeee.Repositories.Extensions;
using System.Data;
using Wheeeee.Core.Classes.Collections;

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

        public IActionResult GetParticipants(string o, int s)
        {
            Guid orchestraGuid = Guid.Parse(o);
            var repo = (ICalenderRepository)Request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));
            var participants = repo.GetParticipants(orchestraGuid, s)
                .Select((p, idx) => new {
                    Index = idx,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Roles = p.RolesIDs ?? Array.Empty<int>(),
                    Instruments = p.InstrumentsIDs ?? Array.Empty<int>()
                })
                .ToArray();

            // Return JSON with original (PascalCase) property names so the client-side script
            // which expects PascalCase (e.g., FirstName) keeps working. The app uses Newtonsoft
            // in views, so serialize with Newtonsoft here to preserve property names instead
            // of relying on the global System.Text.Json naming policy (which may use camelCase).
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(participants);
            return Content(json, "application/json");
        }

        // SaveParticipants removed - editing is client-side only

        [HttpPost]
        public IActionResult Save(string orchestraId)
        {
            if (!HomeController.GetIsAdmin(Request))
            {
                return Forbid();
            }

            Guid orchestraGuid = Guid.Parse(orchestraId);
            var repo = (ICalenderRepository)Request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));

            // Read seasons from form and convert into IDataCollection entries
            var form = Request.Form;
            var seasons = new List<IDataCollection>();

            for (int i = 0; i < 100; i++)
            {
                var dc = CreateSeasonData(form, i);
                if (dc != null) seasons.Add(dc);
            }

            repo.SaveEditableOrchestra(orchestraGuid, seasons);

            return NoContent();
        }

        private IDataCollection CreateSeasonData(Microsoft.AspNetCore.Http.IFormCollection form, int index)
        {
            var prefix = $"Seasons[{index}]";
            var hasAny = form.ContainsKey(prefix + ".ID") || form.ContainsKey(prefix + ".Caption") || form.ContainsKey(prefix + ".StartDate");
            if (!hasAny) return null;

            var idStr = form[prefix + ".ID"].FirstOrDefault() ?? "0";
            var caption = form[prefix + ".Caption"].FirstOrDefault() ?? string.Empty;
            var startStr = form[prefix + ".StartDate"].FirstOrDefault();
            var comment = form[prefix + ".Comment"].FirstOrDefault() ?? string.Empty;
            var isActiveStr = form[prefix + ".IsActive"].FirstOrDefault() ?? "false";

            // skip empty rows (no caption and ID == 0)
            if (string.IsNullOrWhiteSpace(caption) && (idStr == "0" || string.IsNullOrEmpty(idStr)))
            {
                return null;
            }

            var dc = DataCollection.Empty();
            dc.Add("ID", int.TryParse(idStr, out var idVal) ? idVal : 0);
            dc.Add("Caption", caption ?? string.Empty);
            dc.Add("StartDate", DateTime.TryParse(startStr, out var sdt) ? (DateTime?)sdt : null);
            dc.Add("Comment", comment ?? string.Empty);
            dc.Add("IsActive", (isActiveStr ?? string.Empty).ToLower() == "true");

            return dc;
        }
    }
}