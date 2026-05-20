using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IMembership
    {
        IPerson Person { get; }
        IOrchestra Orchestra { get; }
        IEnumerable<IRole> Roles { get; }
        IEnumerable<IInstrument> Instruments { get; }
        Guid UniqueID { get; }
        bool IsAdmin { get; }
        bool HasPlayingMembership { get; }
        CanEditOthers CanEditOthers { get; }
        bool RehearsalVisible { get; }
        ISeason Season { get; }
    }
}