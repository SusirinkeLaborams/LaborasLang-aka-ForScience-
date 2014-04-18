using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests
{
    public class TestBase
    {
        protected static Lexer lexer = new Lexer();

        public TestBase()
        {
            var compilerArgs = CompilerArguments.Parse(new[] { "dummy.il" });
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
        }

        static TestBase()
        {
        }
    }
}
