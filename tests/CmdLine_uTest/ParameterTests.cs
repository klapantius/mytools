﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using consoleapp.CmdLine;

using NUnit.Framework;


namespace CmdLine_uTest
{
    [TestFixture]
    public class ParameterTests
    {
        [Test]
        public void Test1()
        {
            var sut = new Interpreter();
            sut.Add(new consoleapp.CmdLine.Parameter(new[]{"p1"}, "p1", false, "x")).Requires("p2", "p4");
            sut.Add(new consoleapp.CmdLine.Parameter(new[] { "p2" }, "p2", true, "x"));
            sut.Add(new consoleapp.CmdLine.Parameter(new[] { "p3" }, "p3", false, "x"));
            sut.Add(new consoleapp.CmdLine.Parameter(new[] { "p4" }, "p4", false, "x"));

            Assert.IsFalse(sut.Parse("/p1:foo"), "Unexpected result of parsing while required co-parameters are missing.");
            Assert.AreEqual(1, sut.Errors.Count(), "Unexpected result of parsing errors.");
            StringAssert.AreEqualIgnoringCase("Parameter \"p1\" requires parameter \"p2\".", sut.Errors.First(), "Unexpected error message.");
        }

    }
}