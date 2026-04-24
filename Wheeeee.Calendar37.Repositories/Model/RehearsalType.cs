using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class EventType : IEventType
    {
        public EventType(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public int ID { get; }
        public string Name { get; }
    }
}
