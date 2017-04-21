using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using juba.consoleapp;


namespace rsfainstanalyzer
{
    public class StepTimeAnalyzer
    {
        internal static readonly Regex TimeStampRegex = new Regex(@"\d{2,4}[\.-]\d{2}[\.-]\d{2}[\s+,_]\d{2}:\d{2}:\d{2}([:\.]\d+)*\s");

        public TimeSpan GetScriptDuration(TextReader input)
        {
            if (input == null) return TimeSpan.Zero;

            var firstLineWithTimeStamp = FindNextLineIncludingTimeStamp(input);
            string lastLineWithTimeStamp = string.Empty;
            string line ;
            while ((line = FindNextLineIncludingTimeStamp(input))!=null)
            {
                lastLineWithTimeStamp = line;
            }
            var diff = DifferenceBetweenLines(firstLineWithTimeStamp, lastLineWithTimeStamp);
            Out.Log("\t{0}", diff);
            return diff;
        }

        public List<Result> FindLongestSteps(TextReader input, int maxCount = 10)
        {
            if (input == null) return new List<Result>();

            var diffs = new List<Result>();

            diffs.Add(new Result(new TimeSpan(0), FindNextLineIncludingTimeStamp(input)));
            string currentLine;
            while ((currentLine = FindNextLineIncludingTimeStamp(input)) != null)
            {
                var d = DifferenceBetweenLines(diffs.Last().Step, currentLine);
                if (d > TimeSpan.FromDays(1))
                {
                    var x = Regex.Replace(currentLine, @"(?<first>\d{2})(?<middle>\.\d{2}\.)(?<last>\d{2})", 
                        match => string.Format("{0}{1}{2}", match.Groups["last"], match.Groups["middle"], match.Groups["first"]));
                    d = DifferenceBetweenLines(diffs.Last().Step, x);
                }
                diffs.Last().Duration = d;
                diffs.Add(new Result(new TimeSpan(0), currentLine));
            }
            if (diffs.Any() && diffs.Last().Duration == new TimeSpan(0)) diffs.Remove(diffs.Last());

            return diffs
                .OrderByDescending(d => d.Duration)
                .Take(maxCount)
                .ToList();
        }

        public TimeSpan DifferenceBetweenLines(string line1, string line2)
        {
            var time1 = ExtractTimeStamp(line1);
            var time2 = ExtractTimeStamp(line2);
            if (time1 == DateTime.MinValue || time2 == DateTime.MinValue) return new TimeSpan(0);
            //todo: move time correction logic with year-day switch to this place
            return time2 - time1;
        }

        internal DateTime ExtractTimeStamp(string line)
        {
            var result = DateTime.MinValue;
            if (line == null || !TimeStampRegex.IsMatch(line)) return result;
            var x = TimeStampRegex.Matches(line)[0].Value.Replace("_", " ");
            // starter.exe prints a colon before milliseconds, while the standard delimiter is the dot
            if (!DateTime.TryParse(x, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out result))
            {
                if (x.Contains(":"))
                {
                    var y = x.ToCharArray();
                    y[x.LastIndexOf(":")] = '.';
                    x = string.Join("", y);
                }
                DateTime.TryParse(x, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out result);
            }
            return result;
        }

        internal string FindNextLineIncludingTimeStamp(TextReader input)
        {
            string line;

            do
            {
                line = input.ReadLine();
            } while (line != null && !TimeStampRegex.IsMatch(line));

            return line != null ? new Regex(@"\s{2,}").Replace(line, " ") : null;
        }

        public class Result
        {
            public TimeSpan Duration { get; set; }
            public string Step { get; private set; }

            public Result(TimeSpan duration, string step)
            {
                Duration = duration;
                Step = step;
            }
        }

    }
}
