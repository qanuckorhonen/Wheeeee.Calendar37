namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IPersonInstrument
    {
        IPerson Person { get; }
        IInstrument[] Instruments { get; }
        IOrchestra Orchestra { get; }
    }
}
