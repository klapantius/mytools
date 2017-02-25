using System.Linq;
using System.Xml.Linq;
using juba.tfs.interfaces;

namespace sybi
{
    public interface IVersionInfo
    {
        string ScpName { get; }
        string Version { get; }
    }
    public class VersionInfo
    {
        private const string ConstantPartOfFileName = "_VersionInformation";
        private IItem myItem;
        private string scpName;
        public string ScpName
        {
            get
            {
                if (string.IsNullOrEmpty(scpName))
                {
                    var fname = myItem.ServerItem.Split('/').Last().Split('.').FirstOrDefault(x => x.Contains(ConstantPartOfFileName));
                    if (!string.IsNullOrEmpty(fname)) scpName=fname.Replace(ConstantPartOfFileName, "");
                } return scpName;
            }
            internal set { scpName = value; }
        }

        private string version;
        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    var xd = XDocument.Load(myItem.DownloadFile());
                    version = xd
                        .Descendants().ToList()
                        .First(d => d.Name.LocalName == Configuration.Data.VersionNodeName)
                        .Value;
                }
                return version;
            }
            internal set { version = value; }
        }
        public VersionInfo(IItem item)
        {
            myItem = item;
        }
    }
}
