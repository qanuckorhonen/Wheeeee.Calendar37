using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Instrument : IInstrument
    {
        public Instrument(int id, string name, int order, string registerName, int registerOrder, string groupName, int groupOrder)
        {
            ID = id;
            Name = name;
            Order = order;
            RegisterName = registerName;
            RegisterOrder = registerOrder;
            GroupName = groupName;
            GroupOrder = groupOrder;
        }

        public int ID { get; }
        public string Name { get; }
        public int Order { get; }
        public string RegisterName { get; }
        public int RegisterOrder { get; }
        public string GroupName { get; }
        public int GroupOrder { get; }
    }
}