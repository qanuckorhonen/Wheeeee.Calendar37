using System;
using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Person : IPerson
    {
        public Person(int id, Guid uniqueID, string firstName, string lastName, IEnumerable<IMembership> memberships = null)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            UniqueID = uniqueID;
            Memberships = memberships?.ToArray() ?? [];
        }

        public int ID { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public Guid UniqueID { get; }
        public IEnumerable<IMembership> Memberships { get; set; }
    }
}
