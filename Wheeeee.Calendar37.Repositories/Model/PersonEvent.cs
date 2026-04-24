using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class PersonEvent : IPersonEvent
    {
        public PersonEvent(int personID, int eventID, IsPresent isPresent)
        {
            PersonID = personID;
            EventID = eventID;
            IsPresent = isPresent;
        }

        public int PersonID { get; }
        public int EventID { get; }
        public IsPresent IsPresent { get; }
    }
}
