using juba.tfs.interfaces;


namespace sybi
{
    public interface ISourceControlProjectFinder
    {
        ISourceControlProject Find(string path);
        ISourceControlProject Find(IItem item);
    }
}