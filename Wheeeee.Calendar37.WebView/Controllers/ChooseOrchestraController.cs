using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.WebView.Controllers
{
    public class ChooseOrchestraController : Controller
    {
        private readonly ICalenderRepository _repository;

        public ChooseOrchestraController(ICalenderRepository repository)
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
                if (memberships.Length == 1)
                {
                    return new RedirectResult($"/ShowRehearsels/Index?m={memberships[0].UniqueID}");
                }

                return View(memberships);
            }
        }
    }
}
