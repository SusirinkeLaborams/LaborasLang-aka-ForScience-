using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class ConstructorTests : ILTestBase
    {
        [TestMethod]
        public void TestCanEmit_InstanceFieldInitializer()
        {
            var intType = assemblyEmitter.ImportType(typeof(int));

            var initializer = new LiteralNode()
            {
                ReturnType = intType,
                Value = 2
            };

            var field = new FieldDefinition("testField", FieldAttributes.FamANDAssem | FieldAttributes.Family, intType);

            typeEmitter.AddField(field, initializer);

            ExpectedILFilePath = "TestCanEmit_InstanceFieldInitializer.il";
            Test();
        }
    }
}
