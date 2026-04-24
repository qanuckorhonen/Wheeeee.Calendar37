using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IPersonEvent
    {
        int PersonID { get; }
        int EventID { get; }
        IsPresent IsPresent { get; }
    }
}
