using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class PersonRole : IPersonRole
    {
        public PersonRole(int id, string name, string parameters)
        {
            ID = id;
            Name = name;
            Parameters = parameters;
            IsAvtive = true;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public bool IsAvtive { get; set; }
    }
}
