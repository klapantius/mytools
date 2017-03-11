using juba.tfs.interfaces;


namespace sybi
{
    public class BranchFinder: IBranchFinder
    {
        private readonly IExtendedVersionControlServer myVCS;

        public BranchFinder(IExtendedVersionControlServer vcs)
        {
            myVCS = vcs;
        }

        public IBranch Find(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new SybiException("There was no branch on the path.");
            var item = myVCS.GetItem(path);
            return item.IsBranch ? new Branch(item.ServerItem) : Find(ParentPathOf(item.ServerItem));
        }

        public IBranch Find(IItem item)
        {
            return Find(item.ServerItem);
        }
   
        internal string ParentPathOf(string path)
        {
            return path.Substring(0, path.Trim('/').LastIndexOf('/')).Trim('/');
        }

    }
}
