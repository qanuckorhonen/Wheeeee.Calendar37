using System.Drawing.Imaging;
using System.Reflection.Metadata;
using Wheeeee.Calendar37.BL.Model;
using Wheeeee.Calendar37.Core.Enums;
using Wheeeee.Calendar37.Core.Interfaces;
using Wheeeee.Core;
using Wheeeee.Core.Extensions;

namespace Wheeeee.Calendar37.BL
{
    public static class CalendarRenderer
    {
        public static string RenderCalendar(IEnumerable<IPersonInstrument> persons, IEnumerable<IPersonInstrument> otherPersons, IEnumerable<IDatesByWeek> datesByWeek, IEnumerable<IPersonEvent> attendences)
        {
            return $@"
<table>
<tbody>
{RenderWeekRow(datesByWeek)}
{RenderDaysRow(datesByWeek)}
{CalendarCalculator.GetOrchestras(datesByWeek).Select(o => RenderOrchestraRow(persons.Single(p => p.Orchestra.UniqueID == o.UniqueID), otherPersons, datesByWeek, attendences, o)).JoinCrLf()}
</tbody>
</table>
";
        }

        public static string GetAttendenceHtml(IsPresent? isPresent, bool canEdit, bool isOwn, int personID, int dateID)
        {
            var backgroundColor = GetCellBackgroundColor(isPresent, canEdit, isOwn);
            if (!canEdit)
            {
                return $"<td style=\"{backgroundColor}\">&nbsp;</td>";
            }

            var id = $"att_{personID}_{dateID}";

            var content = $@"&nbsp;
<i class=""bi bi-check-circle-fill"" style=""color: #C0FFC0;"" {GetOnClick(isPresent, canEdit, IsPresent.Yes, personID, dateID, id)}></i>
<i class=""bi bi-x-circle"" style=""color: #FFC0C0;"" {GetOnClick(isPresent, canEdit, IsPresent.No, personID, dateID, id)}></i>
<i class=""bi bi-question-circle-fill"" style=""color: #C0C0FF;"" {GetOnClick(isPresent, canEdit, IsPresent.Maybe, personID, dateID, id)}></i>
<i class=""bi bi-arrow-right-circle-fill"" style=""color: #FFFFC0;"" {GetOnClick(isPresent, canEdit, IsPresent.Later, personID, dateID, id)}></i>
&nbsp;
"
                .SurroundWith($@"<span id=""{id}"">", "</span>");

            return (content ?? string.Empty)
                .SurroundWith($"<td align=\"center\" style=\"{backgroundColor}\">", "</td>");
        }

        private static string RenderWeekRow(IEnumerable<IDatesByWeek> datesByWeek)
        {
            return "<tr><td/><td/><td/>" +
                datesByWeek
                .Select(x =>
                {
                    var count = x.Dates
                        .Select(d => d.Date.DateAt.Date)
                        .Distinct()
                        .Count();
                    return $"<td colspan=\"{count}\" style=\"text-align:center;border-left: 1px solid black;\" >{x.Year}<br/>KW {x.Week}</td>";
                })
                .Join()
                + "</tr>";
        }

        private static string RenderDaysRow(IEnumerable<IDatesByWeek> datesByWeek)
        {
            using (CultureRange.German())
            {
                return "<tr><td/><td/><td/>" +
                    CalendarCalculator.GetDates(datesByWeek)
                    .Select(d => d.ToString("d").SurroundWith("<th style=\"text-align:center;border-left: 1px solid black;\">", "</th>"))
                    .Join()
                    + "</tr>";
            }
        }

        private static string RenderOrchestraRow(IPersonInstrument personInstrument, IEnumerable<IPersonInstrument> otherPersonInstruments, IEnumerable<IDatesByWeek> datesByWeek, IEnumerable<IPersonEvent> attendences, IOrchestra orchestra)
        {
            var atts = attendences.ToArray();
            var opis = otherPersonInstruments?.Where(opi => opi.Orchestra.UniqueID == orchestra.UniqueID).ToArray() ?? Array.Empty<IPersonInstrument>();
            var myInstrumentRegisterNames = personInstrument.Instruments.Select(ii => ii.RegisterName).ToArray();
            var myInstrumentGroupNames = personInstrument.Instruments.Select(ii => ii.GroupName).ToArray();

            int rowCount = opis.Length + 1;
            return RenderPersonAttendendanceRow(personInstrument, datesByWeek, atts, orchestra, true, true, opis.Length == 0, true)
                .Replace("[!XXX]", rowCount.ToString()) + $@"
{opis
    .OrderBy(opi => opi.Instruments.FirstOrDefault()?.GroupOrder)
    .ThenBy(opi => opi.Instruments.FirstOrDefault()?.RegisterOrder)
    .ThenBy(opi => opi.Instruments.FirstOrDefault()?.Order)
    .ThenBy(opi => opi.Person.FirstName)
    .Select((opi, index) =>
    {
        bool canEdit = false;
        switch (personInstrument.Person.CanEditOthers)
        {
            case CanEditOthers.all:
                canEdit = true;
                break;
            case CanEditOthers.register:
                canEdit = opi.Instruments.Any(i => i.RegisterName.In(myInstrumentRegisterNames));
                break;
            case CanEditOthers.group:
                canEdit = opi.Instruments.Any(i => i.GroupName.In(myInstrumentGroupNames));
                break;
            default:
                break;
        }

        return RenderPersonAttendendanceRow(opi, datesByWeek, atts, orchestra, canEdit, false, index == opis.Length - 1, false);
    }).Join()}
";
        }

        private static string RenderPersonAttendendanceRow(IPersonInstrument personInstrument, IEnumerable<IDatesByWeek> datesByWeek, IEnumerable<IPersonEvent> atts, IOrchestra orchestra, bool canEdit, bool isFirstRow, bool isLastRow, bool isOwn)
        {
            return @$"<tr style=""{(isFirstRow ? "border-top: 2px solid black; " : string.Empty)}{(isLastRow ? "border-bottom: 1px solid black;" : string.Empty)}""
>{(isFirstRow
                    ? @$"<td rowspan=""[!XXX]""><span style=""writing-mode: sideways-lr; white-space: nowrap;"">{orchestra.Name.Replace(" ", "&nbsp;")}</span>"
                    : string.Empty)}<th>{personInstrument.Person.FirstName}</th><td>({personInstrument.Instruments.Select(i => i.Name).Distinct().OrderBy(n => n).Join(", ")})</td>" +
                            datesByWeek.SelectMany(x => x.Dates.Select(d => d.Date.DateAt.Date))
                                .Distinct()
                                .OrderBy(d => d)
                                .Select(d =>
                                {
                                    var dates = datesByWeek
                                        .SelectMany(x => x.Dates)
                                        .Where(x => x.Date.DateAt.Date == d)
                                        .Where(x => x.Orchestra.UniqueID == orchestra.UniqueID)
                                        .ToArray();

                                    if (dates.Length == 0)
                                    {
                                        return "<td>&nbsp;</td>";
                                    }

                                    var date = dates[0];
                                    var attendances = atts.Where(x => x.EventID == date.Date.ID && x.PersonID == personInstrument.Person.ID).ToArray();
                                    var attendence = atts.FirstOrDefault(x => x.EventID == date.Date.ID && x.PersonID == personInstrument.Person.ID);
                                    var canEditThis = canEdit && date.Date.DateAt.Date >= DateTime.Today;

                                    return GetAttendenceHtml(attendence?.IsPresent, canEditThis, isOwn, personInstrument.Person.ID, date.Date.ID);
                                })
                                .Join()
                                + "</tr>";
        }

        private static string GetCellBackgroundColor(IsPresent? isPresent, bool canEditThis, bool isOwn)
        {
            if (!isPresent.HasValue)
            {
                return isOwn ? $"background-color: {(canEditThis ? "#C0C0C0" : "#E0E0E0")};" : string.Empty;
            }

            return isPresent switch
            {
                IsPresent.No => $"background-color: {(canEditThis ? "#800000" : "#FFC0C0")};",
                IsPresent.Yes => $"background-color: {(canEditThis ? "#008000" : "#C0FFC0")};",
                IsPresent.Maybe => $"background-color: {(canEditThis ? "#000080" : "#C0C0FF")};",
                IsPresent.Later => $"background-color: {(canEditThis ? "#808000" : "#808060")};",
                _ => isOwn ? $"background-color: {(canEditThis ? "#C0C0C0" : "#E0E0E0")};" : string.Empty
            };
        }

        private static string GetOnClick(IsPresent? isPresent, bool canEdit, IsPresent setTo, int personID, int dateID, string id)
        {
            if (isPresent.HasValue && isPresent.Value == setTo || !canEdit)
            {
                return string.Empty;
            }

            return $"onclick=\"setTo('{setTo}', {personID}, {dateID}, '{id}');\"";
        }

        private static string? GetAttendenceString(IsPresent? isPresent)
        {
            switch (isPresent)
            {
                case null:
                    return string.Empty;
                case IsPresent.No:
                    return "nein";
                case IsPresent.Yes:
                    return "ja";
                case IsPresent.Later:
                    return "später";
                case IsPresent.Maybe:
                    return "vielleicht";
                default:
                    return "??";
            }
        }
    }
}