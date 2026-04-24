namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IInstrument:IIDName
    {
        int Order { get; }
        string RegisterName { get; }
        int RegisterOrder { get; }
        string GroupName { get; }
        int GroupOrder { get; }
    }
}