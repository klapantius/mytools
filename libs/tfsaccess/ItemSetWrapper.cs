using juba.tfs.interfaces;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Linq;

namespace juba.tfs.wrappers
{
    public class ItemSetWrapper : IItemSet
    {
        private ItemSet myItemSet;
        public ItemSetWrapper(ItemSet iset)
        {
            myItemSet = iset;
        }

        public IItem[] Items { get { return myItemSet.Items.ToList().Select(i => new ItemWrapper(i)).ToArray(); } }

    }
}
