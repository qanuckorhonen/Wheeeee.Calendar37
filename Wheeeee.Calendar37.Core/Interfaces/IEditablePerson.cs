namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IEditablePerson
    {
        string FirstName { get; }
        string LastName { get; }
        IEnumerable<int> RolesIDs { get; }
    }
}