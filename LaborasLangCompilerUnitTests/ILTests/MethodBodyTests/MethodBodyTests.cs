using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Misc;
using LaborasLangCompiler.Parser.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    class MethodBodyTests
    {
        private string ExpectedIL { get; set; }
        private ICodeBlockNode BodyCodeBlock { get; set; }

        #region Helpers

        private void Test()
        {
            var compilerArgs = CompilerArguments.Parse(new [] { "dummy.il" });
            var assemblyRegistry = new AssemblyRegistry(compilerArgs.References);

            var assemblyEmitter = new AssemblyEmitter(compilerArgs);
            var typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
            var methodEmitter = new MethodEmitter(assemblyRegistry, typeEmitter, "dummy", assemblyRegistry.ImportType(typeof(void)));

            methodEmitter.ParseTree(BodyCodeBlock);

            var method = methodEmitter.Get();
            var methodBodyString = Disassembler.DisassembleMethod(method);

            Assert.AreEqual(ExpectedIL, methodBodyString);
        }

        private AssemblyRegistry CreateDefaultAssemblyRegistry()
        {
            var referenceAssembliesPath = Utils.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.5");

            var references = new List<string>
            {
                Path.Combine(referenceAssembliesPath, "mscorlib.dll")
            };

            return new AssemblyRegistry(references);
        }

        #endregion
    }
}
