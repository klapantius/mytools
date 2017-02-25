using fixtrax;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using juba.tfs.wrappers;
using juba.tfs.interfaces;

namespace fixtrax_utest
{
    [TestFixture]
    public class FixTraxTests
    {
        [Test]
        public void SCPNameCanBeFound()
        {
            var sut = new FixTrax();
            sut.KnownSCPCollectionFolders = new[] { "foo", "bar" };
            StringAssert.AreEqualIgnoringCase("walks", sut.GetSCPName(@"$/a/foo/walks/into/a/bar"));
        }

        [Test]
        public void GetSCPNameThrowsExceptionIfNoSCPNameFound()
        {
            var sut = new FixTrax();
            sut.KnownSCPCollectionFolders = new[] { "foo", "bar" };
            Assert.Throws(typeof(FixTraxException), () => sut.GetSCPName(@"$/two/foos/walks/into/a/bar"), "Exception expected in case of known SCP collection folder is at the end of the path.");
            Assert.Throws(typeof(FixTraxException), () => sut.GetSCPName(@"$/two/foos/walks/into/two/bars"), "Exception expected in case of no known SCP collection folder name is on the path.");
        }

        [Test]
        public void GetParentPathTest()
        {
            var sut = new FixTrax();
            StringAssert.AreEqualIgnoringCase("$/a/foo/walks/into/a", sut.ParentPathOf("$/a/foo/walks/into/a/bar"), "error on happy path");
            StringAssert.AreEqualIgnoringCase("$/a/foo/walks/into/a", sut.ParentPathOf("$/a/foo/walks/into/a/bar/"), "error in case of ending with separator");
        }

        [Test]
        public void BranchCanBeFound()
        {
            var aItem = new Mock<IItem>();
            aItem.Setup(foo => foo.IsBranch).Returns(false);
            aItem.Setup(foo => foo.ServerItem).Returns("$/a");
            var bItem = new Mock<IItem>();
            bItem.Setup(foo => foo.IsBranch).Returns(true);
            bItem.Setup(foo => foo.ServerItem).Returns("$/a/b");
            var cItem = new Mock<IItem>();
            cItem.Setup(foo => foo.IsBranch).Returns(false);
            cItem.Setup(foo => foo.ServerItem).Returns("$/a/b/c");
            var dItem = new Mock<IItem>();
            dItem.Setup(foo => foo.IsBranch).Returns(false);
            dItem.Setup(foo => foo.ServerItem).Returns("$/a/b/c/d");
            var vcs = new Mock<IExtendedVersionControlServer>();
            vcs.Setup(foo => foo.GetItem("$/a/b/c/d")).Returns(dItem.Object);
            vcs.Setup(foo => foo.GetItem("$/a/b/c")).Returns(cItem.Object);
            vcs.Setup(foo => foo.GetItem("$/a/b")).Returns(bItem.Object);
            vcs.Setup(foo => foo.GetItem("$/a")).Returns(aItem.Object);
            var sut = new FixTrax();
            sut.VCS = vcs.Object;
            StringAssert.AreEqualIgnoringCase("$/a/b", sut.GetBranchRootPath("$/a/b/c/d"));
        }

    }
}
