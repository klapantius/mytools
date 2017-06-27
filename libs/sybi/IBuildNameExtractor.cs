namespace sybi
{
    public interface IBuildNameExtractor
    {
        string GetBuildName(string pathToDropFolder);
    }
}