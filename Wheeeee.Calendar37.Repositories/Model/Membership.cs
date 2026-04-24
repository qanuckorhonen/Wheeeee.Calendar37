using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Membership : IMembership
    {
        public Membership(Guid uniqueID, IPerson person, IOrchestra orchestra, ISeason season, IEnumerable<IInstrument> instruments, IEnumerable<IRole> roles)
        {
            UniqueID = uniqueID;
            Person = person;
            Orchestra = orchestra;
            Season = season;
            Instruments = instruments;
            Roles = roles;
        }

        public Guid UniqueID { get; }
        public IPerson Person { get; }
        public IOrchestra Orchestra { get; }
        public ISeason Season { get; }
        public IEnumerable<IInstrument> Instruments { get; }
        public IEnumerable<IRole> Roles { get; }
    }
}