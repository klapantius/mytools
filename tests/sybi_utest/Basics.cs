using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sybi;
using Configuration = sybi.Configuration;
using System.Diagnostics;

namespace sybi_utest
{
    [TestFixture]
    public class Basics
    {
        [Test]
        public void ConfigurationCanBeRead()
        {
            try
            {
                Assert.IsNotNull(Configuration.Data, "Unexpected result while checking loaded configuration data");
                Assert.IsFalse(string.IsNullOrEmpty(Configuration.Data.TFSUri), "Unexpected result for string.IsNullOrEmpty(sut.Data.TFSUri)");
            }
            catch (Exception e)
            {
                Assert.Fail("{0} caught: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
