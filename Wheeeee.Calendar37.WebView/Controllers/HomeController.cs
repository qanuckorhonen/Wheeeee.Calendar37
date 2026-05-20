using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Calendar37.WebView.Models;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.WebView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICalenderRepository _repository;

        public HomeController(ICalenderRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            //return View();

            var personIDs = Request.Cookies[Constants.Cookies.PersonIDs];
            var person = _repository.GetPersonInfo(personIDs);

            if (person == null)
            {
                return new RedirectResult("/DontKnowYou/Index");
            }
            else
            {
                var memberships = person.SelectMany(p => p.Memberships).ToArray();
                if (memberships.IsNullOrEmpty())
                {
                    return new RedirectResult("/DontKnowYou/Index");
                }

                var membershipCount = memberships.Length;
                if (membershipCount == 1)
                {
                    return new RedirectResult($"/ShowRehearsels/Index?m={memberships[0].UniqueID}");
                }
                else // if (orchestraCount > 1)
                {
                    return new RedirectResult("/ChooseOrchestra/Index");
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Admin()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static bool GetIsAdmin(HttpRequest request)
        {
            var personIDs = request.Cookies[Constants.Cookies.PersonIDs];
            var repository = (ICalenderRepository)request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));
            var person = repository.GetPersonInfo(personIDs);

            return person.Any(p => p.Memberships.Any(m => m.Roles.Any(r => r.IsAdmin)));
        }
    }
}