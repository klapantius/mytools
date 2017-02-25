using System;
using System.IO;
using System.Reflection;
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

    public static class Configuration
    {
        public static string ConfigFileName = "sybi.xml";

        private static ConfigurationData myData;
        public static ConfigurationData Data
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
            internal set { myData = value; }
        }

        internal static void WriteSampleConfigurationFile()
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
