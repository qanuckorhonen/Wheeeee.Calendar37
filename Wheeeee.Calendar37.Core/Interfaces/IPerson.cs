using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IPerson
    {
        int ID { get; }
        Guid UniqueID { get; }
        string FirstName { get; }
        string LastName { get; }
        CanEditOthers CanEditOthers { get; }
    }
}