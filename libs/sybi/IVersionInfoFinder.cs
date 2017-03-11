using juba.tfs.interfaces;


namespace sybi
{
    public interface IVersionInfoFinder
    {
        IVersionInfo FindByChangeset(IChangeset cs);
        IVersionInfo FindByChangesetId(int cs);
        IVersionInfo FindByChangeset(IChangeset cs, string vfolder, string vfile);
        IVersionInfo FindByChangesetId(int cs, string vfolder, string vfile);
    }
}
