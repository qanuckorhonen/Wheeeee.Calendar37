namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IEvent : IDate
    {
        IEventType EventType { get; }
    }
}