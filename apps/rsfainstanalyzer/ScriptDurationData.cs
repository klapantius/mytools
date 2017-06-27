using System;
using sybi;


namespace rsfainstanalyzer
{
    public class ScriptDurationData: IComparable
    {
        private readonly IBuildNameExtractor myBuildNameExtractor;

        internal TimeSpan Duration { get; set; }
        internal string DropFolder { get; set; }

        public ScriptDurationData(IBuildNameExtractor buildNameExtractor)
        {
            myBuildNameExtractor = buildNameExtractor;
        }

        internal string BuildName
        {
            get { return myBuildNameExtractor.GetBuildName(DropFolder); }
        }

        public int CompareTo(object obj)
        {
            var other = (ScriptDurationData) obj;
            if (obj == null) throw new ArgumentException("ScriptDurationData.CompareTo parameter must be type of ScriptDurationData.");
            return Duration.CompareTo(other.Duration);
        }
    }
}
