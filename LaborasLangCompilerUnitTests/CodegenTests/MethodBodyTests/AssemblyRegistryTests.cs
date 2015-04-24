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
        private AssemblyEmitter assembly;

        public AssemblyRegistryTests()
        {
            assembly = CodegenTestBase.CreateTempAssembly();
        }

        [TestMethod, TestCategory("Misc IL Tests")]
        public void Test_GetFieldFromBaseType()
        {
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

        [TestMethod, TestCategory("Misc IL Tests")]
        public void Test_GetIndexerProperty()
        {
            var indexer = AssemblyRegistry.GetIndexerProperty(assembly.TypeSystem.String, new[] { assembly.TypeSystem.Int32 });
            Assert.AreEqual("Chars", indexer.Name);
            Assert.AreEqual(1, indexer.Parameters.Count);
            Assert.AreEqual("System.Int32", indexer.Parameters[0].ParameterType.FullName);

            var arrayListType = AssemblyRegistry.FindType(assembly, "System.Collections.ArrayList");
            indexer = AssemblyRegistry.GetIndexerProperty(arrayListType, new[] { assembly.TypeSystem.Int32 });
            Assert.AreEqual("Item", indexer.Name);
            Assert.AreEqual(1, indexer.Parameters.Count);
            Assert.AreEqual("System.Int32", indexer.Parameters[0].ParameterType.FullName);
        }
    }
}
