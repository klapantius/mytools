using Moq;
using NUnit.Framework;
using sybi;
using System;
using System.IO;
using System.Text;
using juba.tfs.interfaces;
using juba.tfs.wrappers;

namespace sybi_utest
{
    [TestFixture]
    public class VersionInfoTests
    {
        private static readonly string VersionInfoFileContentSample =
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

        [Test]
        public void VersionIdCanBeExtractedFromItemInformation()
        {
            var stream = new MemoryStream(Encoding.Default.GetBytes(VersionInfoFileContentSample));
            var mockedItem = new Mock<IItem>();
            mockedItem.Setup(foo => foo.DownloadFile()).Returns(stream);
            var sut = new VersionInfo(mockedItem.Object);
            StringAssert.AreEqualIgnoringCase("4.8.1701.3001", sut.Id, "Unexpected result of Version.");
        }

        [Test]
        public void VersionFileInformationNameCanBeRetrieved()
        {
            var mockedItem = new Mock<IItem>();
            mockedItem.Setup(foo => foo.ServerItem)
                .Returns("$/syngo.net/Modules/Foundations/PCP/v4/_Globals/VersionInformation/Modules.Foundations_ReferenceVersions.xml");
            var sut = new VersionInfo(mockedItem.Object);
            StringAssert.AreEqualIgnoringCase("Modules.Foundations_ReferenceVersions.xml", sut.VersionFile, "Unexpected value of VersionFile");
            StringAssert.AreEqualIgnoringCase("$/syngo.net/Modules/Foundations/PCP/v4/_Globals/VersionInformation", sut.VersionFolder, "Unexpected value of VersionFolder");
        }

    }
}
