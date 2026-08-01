using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class EditableOrchestra : IEditableOrchestra
    {
        public Guid Id { get; }
        public EditableOrchestra(Guid id, string name, string description, string location, IEnumerable<IEditablePerson> persons, IEnumerable<IPersonRole> roles, IEnumerable<IInstrument> instruments, IOrchestraColors colors, IEnumerable<IAttendenceOption> attendenceOptions, IEnumerable<ISeason> seasons)
        {
            Id = id;
            Name = name;
            Description = description;
            Location = location;
            Persons = persons.NN(nameof(persons));
            Roles = roles.NN(nameof(roles));
            Instruments = instruments.NN(nameof(instruments));
            Colors = colors.NN(nameof(colors));
            AttendenceOptions = attendenceOptions.NN(nameof(attendenceOptions));
            Seasons = seasons.NN(nameof(seasons));
        }

        public string Name { get; }
        public string Description { get; }
        public string Location { get; }
        public IEnumerable<IEditablePerson> Persons { get; }
        public IEnumerable<IPersonRole> Roles { get; }
        public IEnumerable<IInstrument> Instruments { get; }
        public IOrchestraColors Colors { get; }
        public IEnumerable<IAttendenceOption> AttendenceOptions { get; }
        public IEnumerable<ISeason> Seasons { get; }
    }
}