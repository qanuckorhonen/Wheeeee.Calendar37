using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class EditablePerson : IEditablePerson
    {
        public EditablePerson(string firstName, string lastName, IEnumerable<int> rolesIDs, IEnumerable<int> instrumentsIDs = null)
        {
            FirstName = firstName;
            LastName = lastName;
            RolesIDs = rolesIDs ?? Array.Empty<int>();
            InstrumentsIDs = instrumentsIDs ?? Array.Empty<int>();
        }

        public string FirstName { get; }
        public string LastName { get; }
        public IEnumerable<int> RolesIDs { get; }
        public IEnumerable<int> InstrumentsIDs { get; }
    }
}
