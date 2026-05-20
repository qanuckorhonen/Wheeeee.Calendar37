using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class OrchestraContext : IMembership
    {
        public OrchestraContext(Guid? uniqueID, IPerson person, ISeason season, IInstrument[] instruments, IRole[] roles, bool isAdmin, bool hasPlayingMembership, CanEditOthers canEditOthers, bool rehearsalVisible, IOrchestra orchestra)
        {
            UniqueID = uniqueID ?? Guid.Empty;
            Person = person;
            Season = season;
            Instruments = instruments;
            Roles = roles;
            IsAdmin = isAdmin;
            HasPlayingMembership = hasPlayingMembership;
            CanEditOthers = canEditOthers;
            RehearsalVisible = rehearsalVisible;
            Orchestra = orchestra;
        }

        public Guid UniqueID { get; }
        public IPerson Person { get; }
        public ISeason Season { get; }
        public IEnumerable<IInstrument> Instruments { get; }
        public IEnumerable<IRole> Roles { get; }
        public bool IsAdmin { get; }
        public bool HasPlayingMembership { get; }
        public CanEditOthers CanEditOthers { get; }
        public bool RehearsalVisible { get; }
        public IOrchestra Orchestra { get; }

    }
}