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

            var sut = new VersionInfoFinder(mockedVCS.Object);
            var result = sut.FindByChangeset(mockedCSs[1].Object, "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/",
                "Modules.Foundations_ReferenceVersions.xml");

            Assert.IsNotNull(result, "A VersionInfo object was expected as result.");
            Assert.AreEqual(200, result.Item.ChangesetId);
        }

        [Test]
        public void FindByChangesetIdOnly()
        {
            var fileNames = new[] { "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/Modules.Foundations_ReferenceVersions.xml" };
            var mockedCSs = new[]
            {
                new MockedChangeset(100, fileNames),
                new MockedChangeset(150, new []{"$/syngo.net/Modules/Foundations/Main/aBundle/source/aProject/aFile.txt"}),
                new MockedChangeset(200, fileNames),
                new MockedChangeset(300, fileNames),
            };
            var mockedVCS = new Mock<IVersionControlServer>();
            mockedVCS.Setup(foo => foo.QueryHistory("$/syngo.net/Modules/Foundations/Main", It.IsAny<bool>(), It.IsAny<bool>(), mockedCSs[1].Object, It.IsAny<bool>()))
                .Returns(mockedCSs.Select(cs => cs.Object).ToList());

            var sut = new VersionInfoFinder(mockedVCS.Object);
            var result = sut.FindByChangeset(mockedCSs[1].Object, "$/syngo.net/Modules/Foundations/Main/_Globals/VersionInformation/",
                "Modules.Foundations_ReferenceVersions.xml");

            Assert.IsNotNull(result, "A VersionInfo object was expected as result.");
            Assert.AreEqual(200, result.Item.ChangesetId);
        }

    }
}
