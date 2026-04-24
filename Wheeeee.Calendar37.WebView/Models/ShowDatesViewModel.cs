using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Calendar37.Repositories.Model;

namespace Wheeeee.Calendar37.WebView.Models
{
    public class ShowDatesViewModel
    {
        private readonly IDictionary<IOrchestra, IEnumerable<IDate>> _dates;

        public ShowDatesViewModel(IPersonInstrument person, IEnumerable<IPersonInstrument> otherPersons, IOrchestra orchestra, IEnumerable<IDate> dates, IEnumerable<IPersonEvent> attendences)
        {
            PersonInstruments = [person];
            OtherPersons = otherPersons.ToArray();
            Attendences = attendences.ToArray();
            _dates = new Dictionary<IOrchestra, IEnumerable<IDate>> { { orchestra, dates } };
        }

        public ShowDatesViewModel(IEnumerable<IPersonInstrument> persons, IEnumerable<IPersonInstrument> otherPersons, IEnumerable<IOrchestra> orchestras, IEnumerable<IOrchestraDate> dates, IEnumerable<IPersonEvent> attendences)
        {
            PersonInstruments = persons.ToArray();
            OtherPersons = otherPersons.ToArray();
            Attendences = attendences.ToArray();
            _dates = orchestras.ToDictionary(o => o, o => dates.Where(d => d.Orchestra.UniqueID == o.UniqueID).Select(od => od.Date));
        }

        public IEnumerable<IPersonInstrument> PersonInstruments { get; }
        public IEnumerable<IPersonInstrument> OtherPersons { get; }
        public IEnumerable<IPersonEvent> Attendences { get; }

        public IEnumerable<IOrchestra> Orchestras => _dates.Keys
            .Distinct()
            .OrderBy(o => o.Name)
            .ToArray();

        public IEnumerable<IOrchestraDate> Dates => _dates.Keys
            .SelectMany(o => _dates[o].Select(d => new OrchestraDate(o, d)))
            .ToArray();
    }
}
