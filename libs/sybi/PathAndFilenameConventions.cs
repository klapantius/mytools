namespace sybi
{
    public static class PathAndFilenameConventions
    {
        public static readonly string VersionInfoFolderSubpath = @"_Globals/VersionInformation";
        public static readonly string VersionInfoFilePattern = @"{0}.{1}_ReferenceVersions.xml";

        public static string VersionInfoFileNameOf(string scpType, string scpName)
        {
            return string.Format(VersionInfoFilePattern, scpType, scpName);
        }

    }
}
