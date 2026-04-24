namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface ISeason
    {
        int SeasonID { get; }
        string SeasonCaption { get; }
        IOrchestra Orchestra { get; }
    }
}