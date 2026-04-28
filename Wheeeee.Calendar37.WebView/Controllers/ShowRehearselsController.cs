using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Wheeeee.Calendar37.BL;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Repositories.Interfaces;
using Wheeeee.Calendar37.WebView.Models;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.WebView.Controllers
{
    public class ShowRehearselsController : Controller
    {
        private readonly ICalenderRepository _repository;

        public ShowRehearselsController(ICalenderRepository repository)
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
                var membershipID = Request.Query["m"].FirstOrDefault();
                if (membershipID.IsNullOrEmpty())
                {
                    return new RedirectResult("/ChooseOrchestra/Index");
                }

                if (membershipID.Contains(','))
                {
                    var ids = membershipID.Split(',');
                    if (ids.Any(id => !Guid.TryParse(id, out _)))
                    {
                        return new RedirectResult("/ChooseOrchestra/Index");
                    }
                    var memberships = _repository.GetMembershipsByUniqueIDs(ids.Select(id => Guid.Parse(id))).ToArray();

                    var model = new ShowDatesViewModel(
                        _repository.GetPersonInstruments(memberships.Select(m => m.UniqueID)).ToArray(),
                        _repository.GetOtherPersonInstruments(memberships.Select(m => m.UniqueID)).ToArray(),
                        memberships.Select(m => m.Orchestra).ToArray(),
                        _repository.GetOrchestraDates(memberships.Select(m => m.UniqueID)).ToArray(),
                        _repository.GetAttendences(memberships.Select(m => m.UniqueID)).ToArray());
                    return View(model);
                }
                else
                {
                    if (!Guid.TryParse(membershipID, out var guid))
                    {
                        return new RedirectResult("/ChooseOrchestra/Index");
                    }

                    var membership = _repository.GetMembershipByUniqueID(guid);
                    if (membership == null)
                    {
                        return new RedirectResult("/ChooseOrchestra/Index");
                    }
                    else
                    {
                        var model = new ShowDatesViewModel(
                            _repository.GetPersonInstrument(membership.UniqueID),
                            _repository.GetOtherPersonInstruments(membership.UniqueID),
                            membership.Orchestra,
                            _repository.GetOrchestraDates(membership.UniqueID),
                            _repository.GetAttendences(membership.UniqueID.AsArray()));
                        return View(model);
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult SetTo([FromBody] SetToRequest request)
        {
            try
            {
                bool success = true;
                IsPresent? isPresent = request.SetTo.IsNullOrEmpty()
                    ? null
                    : request.SetTo.To<IsPresent?>();
                var html = CalendarRenderer.GetAttendenceHtml(isPresent, true, true, request.PersonID, request.DateID);
                _repository.UpdateAttendence(request.PersonID, request.DateID, isPresent);
                return Ok(new
                {
                    success,
                    html
                });
            }
            catch
            {
                return Ok(new { success = false });
            }
        }
    }
}
