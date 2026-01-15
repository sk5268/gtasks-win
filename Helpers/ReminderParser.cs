using System;
using System.Text.RegularExpressions;

namespace Google_Tasks_Client.Helpers
{
    public static class ReminderParser
    {
        private static readonly Regex ReminderRegex = new Regex(@"\s*@([tT])?(\d{1,4})([apAP])?\s*$", RegexOptions.Compiled);

        public static (string Title, DateTime? ReminderTime) Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (input, null);

            var match = ReminderRegex.Match(input);
            if (!match.Success)
            {
                return (input, null);
            }

            string title = input.Substring(0, match.Index).Trim();
            bool isExplicitTomorrow = match.Groups[1].Success;
            string timeStr = match.Groups[2].Value;
            string ampm = match.Groups[3].Value.ToLower();

            int hour;
            int minute = 0;

            if (timeStr.Length <= 2)
            {
                hour = int.Parse(timeStr);
            }
            else if (timeStr.Length == 3)
            {
                hour = int.Parse(timeStr.Substring(0, 1));
                minute = int.Parse(timeStr.Substring(1));
            }
            else // 4 digits
            {
                hour = int.Parse(timeStr.Substring(0, 2));
                minute = int.Parse(timeStr.Substring(2));
            }

            if (hour > 23 || minute > 59) return (input, null);

            DateTime now = DateTime.Now;
            DateTime resultTime;

            if (!string.IsNullOrEmpty(ampm))
            {
                // Explicit AM/PM
                int militaryHour = hour;
                if (ampm == "a")
                {
                    if (militaryHour == 12) militaryHour = 0;
                }
                else // p
                {
                    if (militaryHour != 12) militaryHour += 12;
                }

                DateTime candidate = DateTime.Today.AddHours(militaryHour).AddMinutes(minute);
                if (isExplicitTomorrow)
                {
                    resultTime = candidate.AddDays(1);
                }
                else
                {
                    if (candidate > now)
                    {
                        resultTime = candidate;
                    }
                    else
                    {
                        resultTime = candidate.AddDays(1);
                    }
                }
            }
            else
            {
                // No AM/PM specified
                if (isExplicitTomorrow)
                {
                    // Default to AM if hour <= 12
                    int militaryHour = hour;
                    // if it is reminder for tomorrow, '@9' should mean 9 am by default
                    // So if hour is 9, it stays 9 (AM). If hour is 13, it stays 13 (PM).
                    resultTime = DateTime.Today.AddDays(1).AddHours(militaryHour).AddMinutes(minute);
                }
                else
                {
                    // Nearest logic
                    if (hour > 12)
                    {
                        // Must be 24h format or just afternoon
                        DateTime candidate = DateTime.Today.AddHours(hour).AddMinutes(minute);
                        if (candidate > now)
                        {
                            resultTime = candidate;
                        }
                        else
                        {
                            resultTime = candidate.AddDays(1);
                        }
                    }
                    else
                    {
                        // Could be AM or PM
                        // Special case for 12: @12 usually means 12pm (noon) or 12am (midnight).
                        // If user says @12, nearest 12.
                        
                        int h1 = (hour == 12) ? 0 : hour; // AM candidate hour
                        int h2 = (hour == 12) ? 12 : hour + 12; // PM candidate hour

                        DateTime amCandidate = DateTime.Today.AddHours(h1).AddMinutes(minute);
                        DateTime pmCandidate = DateTime.Today.AddHours(h2).AddMinutes(minute);

                        if (amCandidate > now)
                        {
                            resultTime = amCandidate;
                        }
                        else if (pmCandidate > now)
                        {
                            resultTime = pmCandidate;
                        }
                        else
                        {
                            resultTime = amCandidate.AddDays(1);
                        }
                    }
                }
            }

            return (title, resultTime);
        }
    }
}
