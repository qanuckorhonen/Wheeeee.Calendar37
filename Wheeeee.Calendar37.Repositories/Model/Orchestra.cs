using System;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Orchestra : IOrchestra
    {

        public Orchestra(int id, Guid uniqueID, string name)
        {
            ID = id;
            UniqueID = uniqueID;
            Name = name;
        }

        public int ID { get; }
        public string Name { get; }
        public Guid UniqueID { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
