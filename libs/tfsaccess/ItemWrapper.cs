using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfsaccess
{
    public interface IItem
    {
        string ServerItem { get; }
        bool IsBranch { get; }
        int ChangesetId { get; }
        DateTime CheckinDate { get; }
        Stream DownloadFile();
    }
    public class ItemWrapper : IItem
    {
        private Item item;
        private Item Item
        {
            get
            {
                if (item == null) throw new TFSAccessException("Not initialized object of type Item.");
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
