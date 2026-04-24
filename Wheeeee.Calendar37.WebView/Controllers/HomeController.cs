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
            var membershipIDs = Request.Cookies[Constants.Cookies.MembershipsIDs];
            if (membershipIDs.IsNullOrEmpty())
            {
                return new RedirectResult("/DontKnowYou/Index");
            }
            else
            {
                var memberships = _repository.GetMembershipsByUniqueIDs(membershipIDs.Split(',').Select(s => Guid.Parse(s))).ToArray();
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
            var membershipIDs = request.Cookies[Constants.Cookies.MembershipsIDs];
            if (membershipIDs.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                var repository = (ICalenderRepository)request.HttpContext.RequestServices.GetService(typeof(ICalenderRepository));
                var memberships = repository.GetMembershipsByUniqueIDs(membershipIDs.Split(',').Select(s => Guid.Parse(s))).ToArray();
                if (memberships.IsNullOrEmpty())
                {
                    return false;
                }
                return memberships.Any(m => m.Roles.Any(r => r.IsAdmin));
            }
        }
    }
}