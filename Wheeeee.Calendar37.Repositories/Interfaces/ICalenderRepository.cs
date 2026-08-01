using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core.Interfaces.Collections;

namespace Wheeeee.Calendar37.Repositories.Interfaces
{
    public interface ICalenderRepository
    {
        IEnumerable<IMembership> GetMembershipsByUniqueIDs(IEnumerable<Guid> membershipIDs);
        IMembership GetMembershipByUniqueID(Guid guid);
        //IPerson GetPersonByUniqueID(string personID);
        IEnumerable<IPerson> GetPersonInfo(string personUniqueIDs);
        //IEnumerable<IDate> GetOrchestraDates(Guid membershipID);
        IEnumerable<IOrchestraDate> GetOrchestraDates(IEnumerable<Guid> membershipIDs);
        IEnumerable<IPersonEvent> GetAttendences(Guid membershipID);
        IEnumerable<IPersonEvent> GetAttendences(IEnumerable<Guid> membershipIDs);
        IPersonInstrument GetPersonInstrument(Guid membershipID);
        IEnumerable<IPersonInstrument> GetPersonInstruments(IEnumerable<Guid> membershipIDs);
        IEnumerable<IPersonInstrument> GetOtherPersonInstruments(Guid membershipID);
        IEnumerable<IPersonInstrument> GetOtherPersonInstruments(IEnumerable<Guid> membershipIDs);
        IEditableOrchestra GetEditableOrchestra(Guid orchestraGuid);
        IEnumerable<IAttendenceOption> LoadAttencenceOptions();
        IOrchestraColors GetOrchestraColors(int orchestraID);
        IEnumerable<IPersonRole> GetRoles(int orchestraID);
        IEnumerable<IEditablePerson> GetParticipants(Guid orchestraGuid, int seasonID);
        void SaveEditableOrchestra(Guid orchestraGuid, IEnumerable<Wheeeee.Core.Interfaces.Collections.IDataCollection> seasons);

        void UpdateAttendence(int personID, int dateID, IsPresent? isPresent);
    }
}
