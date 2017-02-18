using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace sybi
{
    [Serializable, XmlType("Configuration")]
    public class ConfigurationData
    {
        public string TFSUri;
        public string TPC;
        public string TeamProject;
        public string VersionInfoPath;
        public string VersionNodeName;
    }

    public class Configuration
    {
        public static string ConfigFileName = "sybi.xml";

        private ConfigurationData myData;
        public ConfigurationData Data
        {
            get
            {
                if (myData == null)
                {
                    var serializer = new XmlSerializer(typeof(ConfigurationData));
                    var str = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    if (str.StartsWith(@"file:\"))
                    {
                        str = str.Substring(6);
                    }
                    //WriteSampleConfigurationFile();
                    using (Stream stream = File.Open(Path.Combine(str, ConfigFileName), FileMode.Open))
                    {
                        myData = (ConfigurationData)serializer.Deserialize(stream);
                    }
                }
                return myData;
            }
        }

        internal void WriteSampleConfigurationFile()
        {
            var sampleData = new ConfigurationData()
            {
                TeamProject = "syngo.net",
                TFSUri = "https://tfs.healthcare.siemens.net:8090/tfs",
                TPC = "IKM.TPC.Projects",
                VersionNodeName = "Version",
                VersionInfoPath = "_Globals/VersionInformation"
            };
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            var str = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (str.StartsWith(@"file:\"))
            {
                str = str.Substring(6);
            }
            TextWriter writer = new StreamWriter(Path.Combine(str, "sybi.sample.xml"));
            serializer.Serialize(writer, sampleData);

        }
    }
}
