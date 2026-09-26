using System;

namespace Watch.Common
{
    public class HandlerDateTime
    {
        private const int HOURS_PER_HALF_DAY = 12;
        private const int MAX_HOURS_VALUE = 23;
        private const int MAX_MINUTES_AND_SECONDS_VALUE = 59;

        public bool TryParseTime(string input, DateTime datePart, out DateTime result)
        {
            result = datePart;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var parts = input.Trim().Split(':');

            if (parts.Length < 2 || parts.Length > 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var hours) ||
                !int.TryParse(parts[1], out var minutes))
            {
                return false;
            }

            var seconds = 0;

            if (parts.Length == 3 && !int.TryParse(parts[2], out seconds))
            {
                return false;
            }

            var isHoursFalse = hours < 0 || hours > MAX_HOURS_VALUE;
            var isMinutesFalse = minutes < 0 || minutes > MAX_MINUTES_AND_SECONDS_VALUE;
            var isSecondsFalse = seconds < 0 || seconds > MAX_MINUTES_AND_SECONDS_VALUE;

            if (isHoursFalse || isMinutesFalse || isSecondsFalse)
            {
                return false;
            }

            result = new DateTime(datePart.Year, datePart.Month, datePart.Day, hours, minutes, seconds);
            return true;
        }

        public int CombineHourWithPeriod(int hour12, int previousHour24)
        {
            var isPm = previousHour24 >= HOURS_PER_HALF_DAY;

            if (hour12 == 0)
            {
                return isPm ? HOURS_PER_HALF_DAY : 0;
            }

            return isPm ? hour12 + HOURS_PER_HALF_DAY : hour12;
        }
    }
}
