namespace Wheeeee.Calendar37.Core.Interfaces
{
    public interface ISeason
    {
        int SeasonID { get; }
        string SeasonCaption { get; }
        DateTime? StartDate { get; }
        DateTime? EndDate { get; }
        IOrchestra Orchestra { get; }
        bool IsCurrent { get; }
    }
}