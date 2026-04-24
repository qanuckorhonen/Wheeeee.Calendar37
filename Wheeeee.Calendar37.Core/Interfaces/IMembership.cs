namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface IMembership
    {
        IPerson Person { get; }
        IOrchestra Orchestra { get; }
        IEnumerable<IRole> Roles { get; }
        IEnumerable<IInstrument> Instruments { get; }
        Guid UniqueID { get; }
    }
}