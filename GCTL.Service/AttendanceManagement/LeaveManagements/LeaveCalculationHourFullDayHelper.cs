using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements
{
    public class LeaveCalculationHelper
    {

       
            public static string FormatLeaveTime(int days, int hours, int minutes)
            {
                var parts = new List<string>();

                if (days > 0)
                    parts.Add($"{days} {(days == 1 ? "Day" : "Days")}");

                if (hours > 0)
                    parts.Add($"{hours} {(hours == 1 ? "Hr" : "Hrs")}");

                if (minutes > 0)
                    parts.Add($"{minutes:D2} min");

                return parts.Count > 0 ? string.Join(", ", parts) : "0 min";
            }

            public static string FormatLeave(decimal totalDaysDecimal, decimal hoursPerDay = 8)
            {
                int days = (int)Math.Floor(totalDaysDecimal);
                decimal remainingHoursDecimal = (totalDaysDecimal - days) * hoursPerDay;

                int hours = (int)Math.Floor(remainingHoursDecimal);
                int minutes = (int)Math.Round((remainingHoursDecimal - hours) * 60);

                // Normalize minutes to hours if needed
                if (minutes >= 60)
                {
                    hours += minutes / 60;
                    minutes %= 60;
                }

                // Normalize hours to days if needed
                if (hours >= (int)hoursPerDay)
                {
                    days += hours / (int)hoursPerDay;
                    hours %= (int)hoursPerDay;
                }

                //  string dayLabel = days == 1 ? "Day" : "Days";
                //string hourLabel = hours == 1 ? "Hr" : "Hrs";

                //return $"{days} {dayLabel}, {hours}{hourLabel}:{minutes:D2} min";
                return $"{days} Day, {hours}:{minutes:D2} Hr";
            }

            public static (string takenFormatted, string remainingFormatted) CalculateTakenAndRemaining(
                decimal? totalLeave,
                decimal? takenDays,
                decimal? takenPartialHours,
                decimal hoursPerDay = 8)
            {
                decimal fullTaken = takenDays ?? 0;
                decimal partialAsDay = (takenPartialHours ?? 0) / hoursPerDay;
                decimal totalTaken = fullTaken + partialAsDay;

                decimal remaining = (totalLeave ?? 0) - totalTaken;

                string takenFormatted = FormatLeave(totalTaken, hoursPerDay);
                string remainingFormatted = FormatLeave(remaining, hoursPerDay);

                return (takenFormatted, remainingFormatted);
            }
        public static double? CalculatePartialHoursTable(TimeOnly? to, TimeOnly? from)
        {
            if (!from.HasValue || !to.HasValue)
                return 0;

            var duration = to.Value.ToTimeSpan() - from.Value.ToTimeSpan();

            if (duration.TotalMinutes <= 0)
                return 0;
            var result = Math.Round(duration.TotalMinutes / 60, 2);
            return result;
        }
    }
}
