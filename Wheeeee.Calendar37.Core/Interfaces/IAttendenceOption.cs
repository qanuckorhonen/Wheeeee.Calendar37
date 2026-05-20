using Wheeeee.Calendar37.Core.Enums;

namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IAttendenceOption
    {
        int ID { get; }
        string AltText { get; }
        IsPresent? Value { get; }
        int OrchestraID { get; }
        string ColorLight { get; }
        string ColorDark { get; }
        string SymbolName { get; }
        string Comment { get; }
        bool IsMandatory { get; }
        int Order { get; }
        bool CanBeFilled { get; }
    }
}
