using System;
using System.Collections.Generic;
using System.Linq;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Orchestra : IOrchestra
    {

        public Orchestra(int id, Guid uniqueID, string name, string color1, IEnumerable<IAttendenceOption> attendenceOptions)
        {
            ID = id;
            UniqueID = uniqueID;
            Name = name;
            AttendenceOptions = attendenceOptions.NN(nameof(attendenceOptions)).ToArray();
            Colors = new OrchestraColors(color1);
        }

        public int ID { get; }
        public string Name { get; }
        public Guid UniqueID { get; }
        public IOrchestraColors Colors { get; }
        public IEnumerable<IAttendenceOption> AttendenceOptions { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
