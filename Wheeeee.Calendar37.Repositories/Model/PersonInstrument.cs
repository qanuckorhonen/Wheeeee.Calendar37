using System;
using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class PersonInstrument : IPersonInstrument
    {
        public PersonInstrument(IPerson person, IEnumerable<IInstrument> instruments, IOrchestra orchestra)
        {
            Person = person;
            Instruments = instruments?.ToArray() ?? Array.Empty<IInstrument>();
            Orchestra = orchestra;
        }

        public IPerson Person { get; }
        public IInstrument[] Instruments { get; }
        public IOrchestra Orchestra { get; }
    }
}
