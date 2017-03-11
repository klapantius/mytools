namespace sybi
{
    public interface ISourceControlProjectFinder
    {
        ISourceControlProject Find(string path);
    }
}