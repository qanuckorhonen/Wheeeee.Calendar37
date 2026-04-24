using System;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Person : IPerson
    {
        public Person(int id, Guid uniqueID, string firstName, string lastName, CanEditOthers canEditOthers)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            UniqueID = uniqueID;
            CanEditOthers = canEditOthers;
        }

        public int ID { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public Guid UniqueID { get; }
        public CanEditOthers CanEditOthers { get; }
    }
}
