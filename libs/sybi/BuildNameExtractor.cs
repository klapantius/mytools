using System.Text.RegularExpressions;


namespace sybi
{
    public class BuildNameExtractor : IBuildNameExtractor
    {
        public const string BuildNamePattern = @"\\(?<buildname>[\w\.]+_[\d\.]+)[\\]?";
        public string GetBuildName(string pathToDropFolder)
        {
            if (string.IsNullOrWhiteSpace(pathToDropFolder)) return string.Empty;
            var r = new Regex(BuildNamePattern);
            return r.IsMatch(pathToDropFolder)
                       ? r.Matches(pathToDropFolder)[0].Groups["buildname"].Value
                       : string.Empty;
        }
    }
}