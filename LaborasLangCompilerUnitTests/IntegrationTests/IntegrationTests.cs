using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    [TestClass]
    public class IntegrationTests : IntegrationTestBase
    {
        [TestMethod, TestCategory("Integration Tests")]
        public void Test_HelloWorld()
        {
            Test("HelloWorld.ll", "Hello, world!" + Environment.NewLine);
        }
    }
}
