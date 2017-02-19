using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfsaccess
{
    public interface IItemSet
    {
        IItem[] Items { get; }
    }
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
