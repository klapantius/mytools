using juba.tfs.interfaces;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.IO;

namespace juba.tfs.wrappers
{
    public class ItemWrapper : IItem
    {
        private Item item;
        private Item Item
        {
            get
            {
                if (item == null) throw new TfsAccessException("Not initialized object of type Item.");
                return item;
            }
            set { item = value; }
        }
        public ItemWrapper(Item item)
        {
            Item = item;
        }

        public string ServerItem { get { return Item.ServerItem; } }
        public bool IsBranch { get { return Item.IsBranch; } }
        public int ChangesetId { get { return Item.ChangesetId; } }
        public DateTime CheckinDate { get { return Item.CheckinDate; } }
        public Stream DownloadFile() { return Item.DownloadFile(); }
    }
}
