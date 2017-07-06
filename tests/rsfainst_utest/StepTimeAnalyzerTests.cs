using System;
using System.IO;

using NUnit.Framework;

using sybi.RSFA;


namespace rsfainst_utest
{
    [TestFixture]
    public class StepTimeAnalyzerTests
    {
        private static readonly string FirstMatchingLine = "STARTER: 16.08.27 12:00:00:000 3435/685 1st line in yymmdd format";
        private static readonly string SecondMatchingLine = "[2016.08.27 12:03:14.000 +02] bar 2nd line in datastorage format";
        private static readonly string ThirdMatchingLine = "STARTER: 27.08.16 12:05:00:000 466/3136 3rd line ddmmyy format";
        private static readonly string mySampleContent =
            "bla" + Environment.NewLine +
            FirstMatchingLine + Environment.NewLine +
            "no timestamp here or there" + Environment.NewLine +
            "bla nr two" + Environment.NewLine +
            SecondMatchingLine + Environment.NewLine +
            "bla three" + Environment.NewLine +
            "bla four" + Environment.NewLine +
            ThirdMatchingLine + Environment.NewLine +
            "404 end of internet" + Environment.NewLine +
            "";


        private static MemoryStream Sample
        {
            get
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(mySampleContent);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }

        private static StreamReader SampleReader { get { return new StreamReader(Sample); } }

        [Test]
        public void FindLongestSteps_HappyPath()
        {
            var sut = new StepTimeAnalyzer();
            var result = sut.FindLongestSteps(SampleReader);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count, "Unexpected count of results.");
            Assert.AreEqual(new TimeSpan(0, 3, 14), result[0].Duration, "Unexpected duration on the 1st place.");
            Assert.AreEqual(FirstMatchingLine, result[0].Step, "Unexpected result[0]");
            Assert.AreEqual(new TimeSpan(0, 1, 46), result[1].Duration, "Unexpected duration on the 2nd place.");
            Assert.AreEqual(SecondMatchingLine, result[1].Step, "Unexpected result[1]");
        }

        [Test]
        public void DifferenceBetweenLines_ReturnsZeroIfFirstLineIsInvalid()
        {
            var sut = new StepTimeAnalyzer();
            Assert.AreEqual(new TimeSpan(0), sut.DifferenceBetweenLines("invalid line", FirstMatchingLine), "Unexpected difference result while the first line is invalid.");
            Assert.AreEqual(new TimeSpan(0), sut.DifferenceBetweenLines(null, SecondMatchingLine), "Unexpected difference result while the first line is null.");
        }

        [Test]
        public void DifferenceBetweenLines_ReturnsZeroIfSecondLineIsInvalid()
        {
            var sut = new StepTimeAnalyzer();
            Assert.AreEqual(new TimeSpan(0), sut.DifferenceBetweenLines(FirstMatchingLine, "invalid line"), "Unexpected difference result while the first line is invalid.");
            Assert.AreEqual(new TimeSpan(0), sut.DifferenceBetweenLines(SecondMatchingLine, null), "Unexpected difference result while the first line is null.");
        }

        [Test]
        public void DifferenceBetweenLines_HappyPath()
        {
            var sut = new StepTimeAnalyzer();
            Assert.AreEqual(new TimeSpan(hours: 0, minutes: 3, seconds: 14), sut.DifferenceBetweenLines(FirstMatchingLine, SecondMatchingLine), "Unexpected difference result on the happy path.");
        }

        [Test]
        public void ExtractTimeStamp_HappyPathWorks()
        {
            var sut = new StepTimeAnalyzer();
            Assert.AreEqual(new DateTime(2016, 08, 27, 12, 0, 0, 0), sut.ExtractTimeStamp(FirstMatchingLine), "Unexpected time value.");
            Assert.AreEqual(new DateTime(2016, 08, 27, 12, 3, 14, 0), sut.ExtractTimeStamp(SecondMatchingLine), "Unexpected time value.");
        }

        [Test]
        public void FindNextLine_Finds1stLine()
        {
            var sut = new StepTimeAnalyzer();
            using (var input = SampleReader)
            {
                var line = sut.FindNextLineIncludingTimeStamp(input);
                Assert.IsNotNull(line, "Unexpected result while searching for the first time stamp.");
                StringAssert.StartsWith(FirstMatchingLine, line, "Unexpected finding while searching for the first time stamp.");
            }
        }

        [Test]
        public void FindNextLine_Finds2ndLine()
        {
            var sut = new StepTimeAnalyzer();
            using (var input = SampleReader)
            {
                sut.FindNextLineIncludingTimeStamp(input);
                var line = sut.FindNextLineIncludingTimeStamp(input);
                Assert.IsNotNull(line, "Unexpected result while searching for the second time stamp.");
                StringAssert.StartsWith(SecondMatchingLine, line, "Unexpected finding while searching for the second time stamp.");
            }
        }

        [Test]
        public void FindNextLine_ReturnsNullAtEnd()
        {
            var sut = new StepTimeAnalyzer();
            using (var input = SampleReader)
            {
                sut.FindNextLineIncludingTimeStamp(input);
                sut.FindNextLineIncludingTimeStamp(input);
                sut.FindNextLineIncludingTimeStamp(input);
                var line = sut.FindNextLineIncludingTimeStamp(input);
                Assert.IsNull(line, "Unexpected result while no more time stamps.");
            }
        }

    }
}
