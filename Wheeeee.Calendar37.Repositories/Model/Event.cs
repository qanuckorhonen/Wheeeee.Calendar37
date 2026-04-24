using System;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Event : DateBase, IEvent
    {
        public Event(int id, DateTime dateAt, string locationAt, IEventType eventType)
            : base(id, dateAt, locationAt)
        {
            EventType = eventType;
        }

        public IEventType EventType { get; }
    }
}
