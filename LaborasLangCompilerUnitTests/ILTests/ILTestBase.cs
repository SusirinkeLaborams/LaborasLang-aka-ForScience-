using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests
{
    public class ILTestBase : TestBase
    {
        protected string ExpectedILFilePath { get; set; }
        internal ICodeBlockNode BodyCodeBlock { get; set; }

        internal CompilerArguments compilerArgs;
        internal TypeEmitter typeEmitter;
        internal AssemblyEmitter assemblyEmitter;

        public ILTestBase()
        {
            var tempLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            compilerArgs = CompilerArguments.Parse(new[] { "dummy.il", "/out:" + tempLocation });
            assemblyEmitter = new AssemblyEmitter(compilerArgs);
            typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
        }

        protected virtual void Test()
        {
            var methodEmitter = new MethodEmitter(typeEmitter, "dummy", AssemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            if (BodyCodeBlock == null)
            {
                BodyCodeBlock = new CodeBlockNode()
                {
                    Nodes = new List<IParserNode>()
                };
            }

            methodEmitter.ParseTree(BodyCodeBlock);
            methodEmitter.SetAsEntryPoint();
            assemblyEmitter.Save();

            var il = Disassembler.DisassembleAssembly(assemblyEmitter.OutputPath);

            var expectedILPath = Path.Combine("..", "..", "ILTests", "MethodBodyTests", "Expected", ExpectedILFilePath);
            var expectedIL = File.ReadAllText(expectedILPath, Encoding.UTF8);

            try
            {
                Assert.AreEqual(expectedIL.Trim(), il.Trim());
            }
            finally
            {
                File.Delete(assemblyEmitter.OutputPath);
                File.Delete(Path.ChangeExtension(assemblyEmitter.OutputPath, ".pdb"));
            }
        }
    }
}
