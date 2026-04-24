using System.Globalization;
using Wheeeee.Calendar37.BL.Model;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.BL
{
    public static class CalendarCalculator
    {
        public static IEnumerable<IDatesByWeek> GetDatesByWeek(IEnumerable<IOrchestraDate> dates)
        {
            return dates
                .GroupBy(d => new { d.Date.DateAt.Year, Week = ISOWeek.GetWeekOfYear(d.Date.DateAt) })
                .Select(g => new DatesByWeek(g.Key.Year, g.Key.Week, g.ToArray()))
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Week)
                .ToArray();
        }

        internal static IEnumerable<IOrchestra> GetOrchestras(IEnumerable<IDatesByWeek> datesByWeek)
        {
            return datesByWeek
                .SelectMany(x => x.Dates.Select(y => y.Orchestra))
                .Distinct()
                .OrderBy(o => o.Name)
                .ToArray();
        }

        internal static IEnumerable<DateTime> GetDates(IEnumerable<IDatesByWeek> datesByWeek)
        {
            return datesByWeek
                .SelectMany(x => x.Dates.Select(y => y.Date.DateAt.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToArray();
        }
    }
}
