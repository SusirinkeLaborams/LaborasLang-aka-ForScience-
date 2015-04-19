using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class AllLexerTests
    {
        [TestMethod, TestCategory("All Tests")]
        public void TestLexer()
        {
            var classes = new List<Type>(){ typeof(RuleValidation), typeof(SyntaxMatcherTests), typeof(TokenizerTests), typeof(ValueBlockTests) };

            classes.ForEach(cls => RunAllFor(cls));
        }

        private void RunAllFor(Type cls)
        {
            var methods = cls.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Any());
            var instance = cls.GetConstructor(new Type[]{}).Invoke(null);

            foreach (var method in methods)
                method.Invoke(instance, null);
        }
    }
}
