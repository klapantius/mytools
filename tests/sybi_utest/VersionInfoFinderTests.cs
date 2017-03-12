using System.Linq;
using juba.tfs.interfaces;
using Moq;
using NUnit.Framework;

using sybi;


namespace sybi_utest
{
    [TestFixture]
    public class VersionInfoFinderTests
    {
        [Test]
        public void FindByChangesetWithPathParameters()
        {
            var fileNames = new[] { "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/Modules.Foundations_ReferenceVersions.xml" };
            var mockedCSs = new[]
            {
                new MockedChangeset(100, fileNames),
                new MockedChangeset(150, fileNames),
                new MockedChangeset(200, fileNames),
                new MockedChangeset(300, fileNames),
            };
            var mockedVCS = new Mock<IVersionControlServer>();
            mockedVCS.Setup(foo => foo.QueryHistory(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IChangeset>(), It.IsAny<bool>()))
                .Returns(mockedCSs.Select(cs => cs.Object).ToList());

            var sut = new VersionInfoFinder(mockedVCS.Object, new BranchFinder(mockedVCS.Object));
            var result = sut.FindByChangeset(mockedCSs[1].Object, "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/",
                "Modules.Foundations_ReferenceVersions.xml");

            Assert.IsNotNull(result, "A VersionInfo object was expected as result.");
            Assert.AreEqual(200, result.Item.ChangesetId);
        }

        [Test]
        public void FindByChangesetOnly()
        {
            var mockedCSs = new[]
            {
                new MockedChangeset(100, new []
                {
                    "$/syngo.net/Modules/Dicom/Main/aBundle/test/aProject/aDicomFile.txt"
                }),
                new MockedChangeset(150, new []
                {
                    "$/syngo.net/Modules/Foundations/Main/aBundle/source/aProject/aFile.txt"
                }),
                new MockedChangeset(200, new []
                {
                    "$/syngo.net/Modules/Foundations/Main/aBundle/source/aProject/anotherFile.txt"
                }),
                new MockedChangeset(300, new []
                {
                    "$/syngo.net/Modules/Foundations/Main/aBundle/source/aProject/aFile.txt",
                    "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/Modules.Foundations_ReferenceVersions.xml",

                }),
            };
            var mockedVCS = new Mock<IVersionControlServer>();
            mockedVCS.Setup(foo => foo.QueryHistory("$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/Modules.Foundations_ReferenceVersions.xml", It.IsAny<bool>(), It.IsAny<bool>(), mockedCSs[1].Object, It.IsAny<bool>()))
                .Returns(new[] { mockedCSs[3] });
            var mockedBranchFinder = new Mock<IBranchFinder>();
            mockedBranchFinder.Setup(foo => foo.Find("$/syngo.net/Modules/Foundations/Main/aBundle/source/aProject/aFile.txt"))
                .Returns(new Branch("$/syngo.net/Modules/Foundations/Main/"));

            var sut = new VersionInfoFinder(mockedVCS.Object, mockedBranchFinder.Object);
            var result = sut.FindByChangeset(mockedCSs[1].Object);

            Assert.IsNotNull(result, "A VersionInfo object was expected as result.");
            Assert.AreEqual(300, result.Item.ChangesetId);
        }

    }
}
