using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class AllParserTests
    {
        [TestMethod, TestCategory("All Tests")]
        public void TestParser()
        {
            var testClass = typeof(ParserTests);
            var methods = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Any()).ToList();
            var instance = Activator.CreateInstance<ParserTests>();
            methods.ForEach(m => m.Invoke(instance, null));
        }
    }
}
