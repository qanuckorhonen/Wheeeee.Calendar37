using System;
using System.Collections.Generic;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class EditableOrchestra : IEditableOrchestra
    {
        public Guid Id => Guid.Empty;
        public string Name => "Orchestra name";
        public string Description => "Orchestra description";
        public string Location => "Orchestra location";
        public IEnumerable<IEditablePerson> Persons => [];

    }
}
