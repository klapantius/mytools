using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using juba.consoleapp.CmdLine;

using NUnit.Framework;


namespace CmdLine_uTest
{
    //[TestFixture]
    public class ParameterTests
    {
        //[Test]
        //public void MissingAssociatedParameterWithoutDefaultValueResultsError()
        //{
        //    var sut = new Interpreter();
        //    sut.Add(new juba.consoleapp.CmdLine.Parameter(new[]{"p1"}, "p1", "description text", false, "x")).Requires("p2", "p4");
        //    sut.Add(new juba.consoleapp.CmdLine.Parameter(new[] { "p2" }, "p2", "description text", false));
        //    sut.Add(new juba.consoleapp.CmdLine.Parameter(new[] { "p3" }, "p3", "description text", false, "x"));
        //    sut.Add(new juba.consoleapp.CmdLine.Parameter(new[] { "p4" }, "p4", "description text", false, "x"));

        //    Assert.IsFalse(sut.Parse("/p1:foo"), "Unexpected result of parsing while required co-parameters are missing.");
        //    Assert.AreEqual(1, sut.Errors.Count(), "Unexpected result of parsing errors.");
        //    StringAssert.AreEqualIgnoringCase("Parameter \"p1\" requires parameter \"p2\".", sut.Errors.First(), "Unexpected error message.");
        //}

    }
}
