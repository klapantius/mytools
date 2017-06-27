using System;
using NUnit.Framework;
using sybi;


namespace sybi_utest
{
    [TestFixture]
    public class BuildNameExtractorTests
    {
        [Test]
        public void WhenValidDropFolderEndsWithBuildName()
        {
            const string buildName = "Modules.Core.Main.SysIntGate.GC_20170623.5";
            const string path = @"\\build-sy.healthcare.siemens.com\dropnative$\" + buildName;
            var sut = new BuildNameExtractor();
            StringAssert.AreEqualIgnoringCase(buildName, sut.GetBuildName(path));
        }
        [Test]
        public void WhenValidDropFolderContinuesAfterBuildName()
        {
            const string buildName = "Modules.Core.Main.SysIntGate.GC_20170623.5";
            const string path = @"\\build-sy.healthcare.siemens.com\dropnative$\" + buildName + @"\logs\x64\Debug\testOutput\bb993635-7603-4e94-8d1b-f9f978267dd2";
            var sut = new BuildNameExtractor();
            StringAssert.AreEqualIgnoringCase(buildName, sut.GetBuildName(path));
        }
        [Test]
        public void WhenThePathIsNullOrEmpty()
        {
            var sut = new BuildNameExtractor();
            StringAssert.AreEqualIgnoringCase(string.Empty, sut.GetBuildName(null), "Failure when the paramter is null.");
            StringAssert.AreEqualIgnoringCase(string.Empty, sut.GetBuildName(""), "Failure when the paramter is an empty string.");
            StringAssert.AreEqualIgnoringCase(string.Empty, sut.GetBuildName(" \t \n\r"), "Failure when the paramter consists whitespaces only.");
        }
    }
}
