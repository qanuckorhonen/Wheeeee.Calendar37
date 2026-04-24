using System.Reflection.Metadata.Ecma335;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.BL.Model
{
    internal class DatesByWeek : IDatesByWeek
    {
        public DatesByWeek(int year, int week, IEnumerable<IOrchestraDate> dates)
        {
            Year = year;
            Week = week;
            Dates = dates;
        }

        public int Year { get; }
        public int Week { get; }
        public IEnumerable<IOrchestraDate> Dates { get; }

        public int CompareTo(IDatesByWeek? other)
        {
            if (other == null)
            {
                return 1;
            }

            if (this == other)
            {
                return 0;
            }

            if (Year == other.Year)
            {
                return Week.CompareTo(other.Week);
            }

            return Year.CompareTo(other.Year);
        }
    }
}
