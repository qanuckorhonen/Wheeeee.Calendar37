using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IRole : IIDName
    {
        bool IsAdmin { get; }
        CanEditOthers CanEditOthers { get; }
        SeesOthers SeesOthers { get; }
    }
}