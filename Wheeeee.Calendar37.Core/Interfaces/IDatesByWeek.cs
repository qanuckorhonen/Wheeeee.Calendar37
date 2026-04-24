namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IDatesByWeek : IComparable<IDatesByWeek>
    {
        int Year { get; }
        int Week { get; }
        IEnumerable<IOrchestraDate> Dates { get; }
    }
}
