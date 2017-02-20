using Moq;
using NUnit.Framework;
using sybi;
using System;
using System.IO;
using System.Text;
using tfsaccess;

namespace sybi_utest
{
    [TestFixture]
    public class VersionInfoTests
    {
        [Test]
        public void ScpNameCanBeExtractedFromItemInformation()
        {
            var mockedItem = new Mock<IItem>();
            mockedItem.Setup(foo => foo.ServerItem).Returns("$/this/is/a/long/path/Modules.Foundations_ReferenceVersions.xml");
            var sut = new VersionInfo(mockedItem.Object);
            StringAssert.AreEqualIgnoringCase("Foundations", sut.ScpName, "Unexpected result of ScpName.");
        }

        [Test]
        public void VersionCanBeExtractedFromItemInformation()
        {
            var xmlString = 
                @"<?xml version=" + "\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                 "<BundleVersionInformation xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/Siemens.TFS.Build.Core.DependencyManagement\">" +
                @" <BundleVersions>
                     <BundleVersion>
                       <Name>RSFA</Name>
                       <Version>4.8.1701.3001</Version>
                     </BundleVersion>
                     <BundleVersion>
                       <Name>RSFA_DevShell</Name>
                       <Version>4.8.1701.3001</Version>
                     </BundleVersion>
                     <BundleVersion>
                       <Name>SDN</Name>
                       <Version>4.8.1701.3001</Version>
                     </BundleVersion>
                   </BundleVersions>
                   <CreatedByBuildName>Modules.Foundations.v4.Nightly.Upload.NB_20170130.1</CreatedByBuildName>
                   <CreatedOnBranch>$/syngo.net/Modules/Foundations/PCP/v4</CreatedOnBranch>
                   <Project>Modules.Foundations</Project>
                 </BundleVersionInformation>";
            var stream = new MemoryStream(Encoding.Default.GetBytes(xmlString));
            var mockedItem = new Mock<IItem>();
            mockedItem.Setup(foo => foo.DownloadFile()).Returns(stream);
            var sut = new VersionInfo(mockedItem.Object);
            StringAssert.AreEqualIgnoringCase("4.8.1701.3001", sut.Version, "Unexpected result of Version.");
        }
    }
}
