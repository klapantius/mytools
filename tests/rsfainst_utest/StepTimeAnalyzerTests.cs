using System;
using System.IO;

using NUnit.Framework;

using rsfainstanalyzer;


namespace rsfainst_utest
{
    [TestFixture]
    public class StepTimeAnalyzerTests
    {
        private static readonly string FirstMatchingLine = "foo 16.08.27 12:00:00:000 bar";
        private static readonly string SecondMatchingLine = "foo 16.08.27 12:03:14.000 bar";
        private static readonly string ThirdMatchingLine = "foo 16.08.27 12:05:00.000 bar";
        private static readonly string SampleContent =
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
                writer.Write(SampleContent);
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
            Assert.AreEqual(FirstMatchingLine, result[0].Step, "Unexpected result[0]");
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
