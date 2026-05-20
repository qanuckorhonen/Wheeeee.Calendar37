using Newtonsoft.Json;
using Wheeeee.Calendar37.Core;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Role : IRole
    {
        public Role(int id, string name, string parametersJson)
        {
            ID = id;
            Name = name;

            RoleParameters parameters = JsonConvert.DeserializeObject<RoleParameters>(parametersJson);
            CanEditOthers = parameters?.CanEditOthers ?? CanEditOthers.no;
            SeesOthers = parameters?.SeesOthers ?? SeesOthers.no;
        }

        public int ID { get; }
        public string Name { get; }
        public CanEditOthers CanEditOthers { get; }
        public SeesOthers SeesOthers { get; }

        public bool IsAdmin => Name == Constants.Roles.Admin;
    }
}
