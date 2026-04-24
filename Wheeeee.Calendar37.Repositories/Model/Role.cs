using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Role : IRole
    {
        public Role(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public int ID { get; }
        public string Name { get; }
        public bool IsAdmin => Name == Constants.Roles.Admin;
    }
}
