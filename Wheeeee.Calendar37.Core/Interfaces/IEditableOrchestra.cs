namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IEditableOrchestra
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        string Location { get; }

        IEnumerable<IEditablePerson> Persons { get; }
        IOrchestraColors Colors { get; }
        IEnumerable<IAttendenceOption> AttendenceOptions { get; }
        IEnumerable<ISeason> Seasons { get; }
    }
}