namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IOrchestra
    {
        int ID { get; }
        string Name { get; }
        Guid UniqueID { get; }
        IOrchestraColors Colors { get; }
        IEnumerable<IAttendenceOption> AttendenceOptions { get; }
    }
}