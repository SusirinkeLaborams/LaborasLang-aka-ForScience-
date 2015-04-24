using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.FrontEnd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.CodegenTests.MethodBodyTests
{
    [TestClass]
    public class AssemblyRegistryTests : TestBase
    {
        [TestMethod, TestCategory("Misc IL Tests")]
        public void Test_GetFieldFromBaseType()
        {
            var tempLocation = Path.Combine(GetTestDirectory(), "TestExecutable.exe");
            var compilerArgs = CompilerArguments.Parse(new[] { "ExecuteTest.il", "/out:" + tempLocation });
            var assembly = new AssemblyEmitter(compilerArgs);

            var baseType = new TypeEmitter(assembly, "BaseType", "", TypeEmitter.DefaultTypeAttributes, assembly.TypeSystem.Object);
            var derivedType = new TypeEmitter(assembly, "DerivedType", "", TypeEmitter.DefaultTypeAttributes, baseType.Get(assembly));

            var expectedField = new FieldDefinition("Field", FieldAttributes.Public, assembly.TypeSystem.Int32);
            baseType.AddField(expectedField);

            var actualField = AssemblyRegistry.GetField(assembly, derivedType, "Field");
            Assert.AreEqual(expectedField, actualField);
        }

        [TestMethod, TestCategory("Misc IL Tests")]
        public void Test_GetPropertyFromBaseType()
        {
            var property = AssemblyRegistry.GetProperty("System.IO.StreamWriter", "NewLine");
            Assert.AreEqual("System.IO.TextWriter", property.DeclaringType.FullName);
            Assert.AreEqual("NewLine", property.Name);
        }
    }
}
