using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    public class TestBase
    {
        public TestBase()
        {
            var compilerArgs = CompilerArguments.Parse(new[] { "dummy.il" });
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
        }
    }
}
