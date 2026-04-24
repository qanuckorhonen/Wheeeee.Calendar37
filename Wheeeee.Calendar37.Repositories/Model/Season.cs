using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Season : ISeason
    {
        public Season(int seasonID, string seasonCaption, IOrchestra orchestra)
        {
            SeasonID = seasonID;
            SeasonCaption = seasonCaption;
            Orchestra = orchestra;
        }

        public int SeasonID { get; }
        public string SeasonCaption { get; }
        public IOrchestra Orchestra { get; }
    }
}