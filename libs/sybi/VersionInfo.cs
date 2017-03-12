using System.Linq;
using System.Xml.Linq;
using juba.tfs.interfaces;

namespace sybi
{
    public class VersionInfo : IVersionInfo
    {
        private string myID;
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(myID))
                {
                    if (Item == null) return string.Empty;
                    var xd = XDocument.Load(Item.DownloadFile());
                    myID = xd
                        .Descendants().ToList()
                        .First(d => d.Name.LocalName == Configuration.Data.VersionNodeName)
                        .Value;
                }
                return myID;
            }
            internal set { myID = value; }
        }

        public IItem Item { get; private set; }

        public VersionInfo(IItem item)
        {
            Item = item;
        }

        public VersionInfo() { }

        public string VersionFolder
        {
            get { return Item.ServerItem.Substring(0, Item.ServerItem.LastIndexOf('/')); }
        }

        public string VersionFile
        {
            get { return Item.ServerItem.Substring(Item.ServerItem.LastIndexOf('/') + 1); }
        }

    }
}
