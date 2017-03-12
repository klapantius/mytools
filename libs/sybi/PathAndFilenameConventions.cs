namespace sybi
{
    public static class PathAndFilenameConventions
    {
        private static readonly string VersionInfoFolderSubpath = @"_Globals/VersionInformation";
        private static readonly string VersionInfoFilePattern = @"{0}.{1}_ReferenceVersions.xml";

        public static string VersionInfoFileNameOf(string scpType, string scpName)
        {
            return string.Format(VersionInfoFilePattern, scpType, scpName);
        }

        public static string AddVersionInfoFolderSubpathTo(string basePath)
        {
            return basePath.EndsWith(VersionInfoFolderSubpath) ? basePath : string.Join("/", basePath.Trim('/'), VersionInfoFolderSubpath);
        }

    }
}
