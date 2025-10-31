using System;
using System.Collections.Generic;
using System.Globalization;

namespace DengueAlert.Api.Helpers
{
    public class EpidemiologicalWeekHelper
    {
        private const DayOfWeek FimDaSemanaSE = DayOfWeek.Saturday;
        public static (int ewStart, int eyStart, int ewEnd, int eyEnd) GetWeekRangeForLastMonths(int months)
        {
            var end = DateTime.UtcNow.Date;
            var start = end.AddMonths(-months);

            var (ewStart, eyStart) = GetEpiWeekForDate(start);
            var (ewEnd, eyEnd) = GetEpiWeekForDate(end);

            return (ewStart, eyStart, ewEnd, eyEnd);
        }

        public static (int ew, int ey) GetEpiWeekForDate(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            int week = cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
            int year = date.Year;

            if (date.Month == 1 && week >= 52) year = date.Year - 1;
            if (date.Month == 12 && week == 1) year = date.Year + 1;

            return (week, year);
        }

        public static IEnumerable<(int ewStart, int ewEnd, int eyStart, int eyEnd)> SplitByYear(int ewStart, int eyStart, int ewEnd, int eyEnd)
        {
            if (eyStart == eyEnd)
            {
                yield return (ewStart, ewEnd, eyStart, eyEnd);
                yield break;
            }

            int lastWeekOfStartYear = GetWeeksInYear(eyStart);
            yield return (ewStart, lastWeekOfStartYear, eyStart, eyStart);

            for (int y = eyStart + 1; y < eyEnd; y++)
            {
                yield return (1, GetWeeksInYear(y), y, y);
            }

            yield return (1, ewEnd, eyEnd, eyEnd);
        }

        public static int GetWeeksInYear(int year)
        {
            var dec31 = new DateTime(year, 12, 31);
            var (w, y) = GetEpiWeekForDate(dec31);
            return w;
        }

        public static DateTime GetUltimoDiaSECompleta(DateTime referenceDate)
        {
            DateTime sabadoAtual = GetDiaDaSemana(referenceDate.Date, FimDaSemanaSE);

            if (referenceDate.Date < sabadoAtual)
            {
                return sabadoAtual.AddDays(-7);
            }

            return sabadoAtual;
        }

        private static DateTime GetDiaDaSemana(DateTime date, DayOfWeek day)
        {
            int diff = (7 + (day - date.DayOfWeek)) % 7;
            return date.AddDays(diff);
        }
    }
}
