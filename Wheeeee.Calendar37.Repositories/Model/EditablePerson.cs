using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class EditablePerson : IEditablePerson
    {
        public EditablePerson(string firstName, string lastName, IEnumerable<int> rolesIDs)
        {
            FirstName = firstName;
            LastName = lastName;
            RolesIDs = rolesIDs;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public IEnumerable<int> RolesIDs { get; }
    }
}
