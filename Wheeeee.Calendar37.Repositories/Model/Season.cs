using System;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class Season : ISeason
    {
        public Season(int seasonID, string seasonCaption, DateTime? startDate, DateTime? endDate, IOrchestra orchestra)
        {
            SeasonID = seasonID;
            SeasonCaption = seasonCaption;
            StartDate = startDate;
            EndDate = endDate;
            Orchestra = orchestra;
        }

        public int SeasonID { get; }
        public string SeasonCaption { get; }
        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }
        public IOrchestra Orchestra { get; }
        public bool IsCurrent => DateTime.Today.IsInRange(StartDate ?? DateTime.MaxValue, EndDate ?? DateTime.MinValue);
    }
}