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

        public static string GetAttendenceHtml(int index, IsPresent? isPresent, bool canEdit, bool isOwn, int personID, int dateID, int orchestraID, IEnumerable<IAttendenceOption> attendenceOptions, IOrchestraColors orchestraColors)
        {
            var backgroundColor = GetCellBackgroundColor(isPresent, canEdit, isOwn, orchestraColors, index);
            if (!canEdit)
            {
                return $"<td style=\"{backgroundColor}\">&nbsp;</td>";
            }

            var id = $"att_{personID}_{dateID}";

            var content = attendenceOptions
                   .OrderBy(ao => ao.Order)
                   .Select(ao =>
                   {
                       var filled = ao.CanBeFilled && isPresent.HasValue && ao.Value == isPresent.Value ? "-fill" : string.Empty;
                       var color = isPresent.HasValue
                       ? ao.ColorLight
                       : ao.Value.HasValue
                            ? ao.ColorDark
                            : backgroundColor;

                       return $@"<i class=""bi {ao.SymbolName}{filled}"" style=""color: #{color};"" {GetOnClick(isPresent, canEdit, ao.Value, personID, dateID, orchestraID, id, index)}></i>";
                   })
                   .Join("&nbsp;");

            content = content
                .Replace("\r\n", "&nbsp;")
                .SurroundWith($@"<span id=""{id}"">&nbsp;", "&nbsp;</span>");

            return (content ?? string.Empty)
                .SurroundWith($@"<td align=""center"" style=""{backgroundColor}; white-space: nowrap;"">", "</td>");
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
                    return $"<td colspan=\"{count}\" style=\"text-align:center;  border-left: 1px solid black; position: sticky; top: 0; background: white; z-index: 1;\" >KW {x.Week}</td>";
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
                    .Select(d => d.ToString("d").SurroundWith("&nbsp;").SurroundWith($"<th style=\"text-align:center; white-space: nowrap; border-left: 1px solid black; position: sticky; top: 0; background: white; z-index: 2; color:#{(d >= DateTime.Today ? "000000" : "D0D0D0")}\">", "</th>"))
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
            return RenderPersonAttendendanceRow(personInstrument, datesByWeek, atts, orchestra, true, true, opis.Length == 0, true, 1)
                .Replace("[!XXX]", rowCount.ToString()) + $@"
{opis
    .OrderBy(opi => opi.Instruments.FirstOrDefault()?.GroupOrder)
    .ThenBy(opi => opi.Instruments.FirstOrDefault()?.RegisterOrder)
    .ThenBy(opi => opi.Instruments.FirstOrDefault()?.Order)
    .ThenBy(opi => opi.Person.FirstName)
    .Select((opi, index) =>
    {
        bool canEdit = false;
        switch (personInstrument.Person.Memberships.Single(m => m.Orchestra.UniqueID == orchestra.UniqueID).CanEditOthers)
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

        return RenderPersonAttendendanceRow(opi, datesByWeek, atts, orchestra, canEdit, false, index == opis.Length - 1, false, index + 1);
    }).Join()}
";
        }

        private static string RenderPersonAttendendanceRow(IPersonInstrument personInstrument, IEnumerable<IDatesByWeek> datesByWeek, IEnumerable<IPersonEvent> atts, IOrchestra orchestra, bool canEdit, bool isFirstRow, bool isLastRow, bool isOwn, int index)
        {
            string backgroundColor = GetCellBackgroundColor(null, false, isOwn, orchestra.Colors, index);
            return @$"<tr style=""{(isFirstRow ? "border-top: 2px solid black;" : string.Empty)}{(isLastRow ? "border-bottom: 1px solid black;" : string.Empty)} position: sticky;left: 0;z-index: 1;background: white;"">{(isFirstRow
                    ? @$"<td rowspan=""[!XXX]"" style=""position: sticky;left: 0;z-index: 1;background: #{orchestra.Colors.ColorHeaderHtml};""><span style=""writing-mode: sideways-lr; white-space: nowrap;"">{orchestra.Name.Replace(" ", "&nbsp;")}</span>"
                    : string.Empty)}<th style=""position: sticky;left: 0;z-index: 1;{backgroundColor}; white-space: nowrap;"">{personInstrument.Person.FirstName}</th><td style=""white-space: nowrap;{backgroundColor}"">({personInstrument.Instruments.Select(i => i.Name).Distinct().OrderBy(n => n).Join(", ")})</td>" +
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
                                        var backgroundColor = GetCellBackgroundColor(null, canEdit, isOwn, orchestra.Colors, index);
                                        return $"<td style=\"{backgroundColor}\">&nbsp;</td>";
                                    }

                                    var date = dates[0];
                                    var attendances = atts.Where(x => x.EventID == date.Date.ID && x.PersonID == personInstrument.Person.ID).ToArray();
                                    var attendence = atts.FirstOrDefault(x => x.EventID == date.Date.ID && x.PersonID == personInstrument.Person.ID);
                                    var canEditThis = canEdit && date.Date.DateAt.Date >= DateTime.Today;

                                    return GetAttendenceHtml(index, attendence?.IsPresent, canEditThis, isOwn, personInstrument.Person.ID, date.Date.ID, orchestra.ID, orchestra.AttendenceOptions, orchestra.Colors);
                                })
                                .Join()
                                + "</tr>";
        }

        private static string GetCellBackgroundColor(IsPresent? isPresent, bool canEditThis, bool isOwn, IOrchestraColors orchestraColors, int index)
        {
            if (!isPresent.HasValue)
            {
                return isOwn
                    ? $"background-color: #{orchestraColors.ColorHeaderHtml};"
                    : index % 2 == 0
                        ? $"background-color: #{orchestraColors.RowColor0Html};"
                        : $"background-color: #{orchestraColors.RowColor1Html};";
            }

            return isPresent switch
            {
                IsPresent.Yes => $"background-color: #{(canEditThis ? "008000" : "C0FFC0")};",
                IsPresent.No => $"background-color: #{(canEditThis ? "800000" : "FFC0C0")};",
                IsPresent.Maybe => $"background-color: #{(canEditThis ? "000080" : "C0C0FF")};",
                IsPresent.Later => $"background-color: #{(canEditThis ? "808000" : "FFFFC0")};",
                _ => throw new NotImplementedException(),
            };
        }

        private static string GetOnClick(IsPresent? isPresent, bool canEdit, IsPresent? setTo, int personID, int dateID, int orchestraID, string id, int index)
        {
            if (isPresent.HasValue && isPresent.Value == setTo || !canEdit || !isPresent.HasValue && setTo == null)
            {
                return string.Empty;
            }

            return $"onclick=\"setTo('{setTo}', {personID}, {dateID}, {orchestraID}, '{id}', {index});\"";
        }
    }
}