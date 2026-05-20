using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Repositories.Interfaces;

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
            var personIDs = Request.Cookies[Constants.Cookies.PersonIDs];
            var person = _repository.GetPersonInfo(personIDs);

            if (person == null)
            {
                return new RedirectResult("/DontKnowYou/Index");
            }
            else
            {
                var memberships = person.SelectMany(p => p.Memberships).ToArray();
                if (memberships.Length == 1)
                {
                    return new RedirectResult($"/ShowRehearsels/Index?m={memberships[0].UniqueID}");
                }

                return View(memberships);
            }
        }
    }
}
