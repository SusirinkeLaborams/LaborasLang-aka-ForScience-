using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaborasLangCompilerUnitTests.ILTests
{
    [TestClass]
    public class ILHelpersTests : TestBase
    {
        private AssemblyEmitter assembly;

        [TestMethod, TestCategory("IL Helpers Tests")]
        public void TestIsAssignableTo()
        {
            var types = new Dictionary<string, TypeReference>();
            Action<string, string> addType = (tag, typeName) => types[tag] = AssemblyRegistry.GetType(assembly, typeName);

            addType("int8", "System.SByte");
            addType("int16", "System.Int16");
            addType("int32", "System.Int32");
            addType("int64", "System.Int64"); ;
            addType("intptr", "System.IntPtr");
            addType("uint8", "System.Byte");
            addType("uint16", "System.UInt16");
            addType("char", "System.Char");
            addType("uint32", "System.UInt32");
            addType("uint64", "System.UInt64");
            addType("uintptr", "System.UIntPtr");
            addType("boolean", "System.Boolean");
            addType("str", "System.String");
            addType("streamwriter", "System.IO.StreamWriter");
            addType("textwriter", "System.IO.TextWriter");
            addType("textreader", "System.IO.TextReader");
            addType("array", "System.Array");
            addType("icloneable", "System.ICloneable");

            foreach (var left in types)
            {
                var exceptions = new List<string>();

                #region Exceptions

                switch (left.Key)
                {
                    case "int8":
                        exceptions.Add("int8");
                        break;

                    case "int16":
                        exceptions.Add("int8");
                        exceptions.Add("int16");
                        break;

                    case "int32":
                        exceptions.Add("char");
                        exceptions.Add("int8");
                        exceptions.Add("uint8");
                        exceptions.Add("int16");
                        exceptions.Add("uint16");
                        exceptions.Add("int32");
                        break;

                    case "int64":
                        exceptions.Add("char");
                        exceptions.Add("int8");
                        exceptions.Add("uint8");
                        exceptions.Add("int16");
                        exceptions.Add("uint16");
                        exceptions.Add("int32");
                        exceptions.Add("uint32");
                        exceptions.Add("int64");
                        break;

                    case "intptr":
                        exceptions.Add("intptr");
                        break;

                    case "uint8":
                        exceptions.Add("uint8");
                        break;

                    case "uint16":
                        exceptions.Add("uint8");
                        exceptions.Add("uint16");
                        exceptions.Add("char");
                        break;

                    case "uint32":
                        exceptions.Add("uint8");
                        exceptions.Add("uint16");
                        exceptions.Add("uint32");
                        exceptions.Add("char");
                        break;

                    case "uint64":
                        exceptions.Add("uint8");
                        exceptions.Add("uint16");
                        exceptions.Add("uint32");
                        exceptions.Add("uint64");
                        exceptions.Add("char");
                        break;

                    case "uintptr":
                        exceptions.Add("uintptr");
                        break;

                    case "char":
                        exceptions.Add("uint8");
                        exceptions.Add("uint16");
                        exceptions.Add("char");
                        break;

                    case "boolean":
                        exceptions.Add("boolean");
                        break;

                    case "str":
                        exceptions.Add("str");
                        break;

                    case "streamwriter":
                        exceptions.Add("streamwriter");
                        break;

                    case "textwriter":
                        exceptions.Add("textwriter");
                        exceptions.Add("streamwriter");
                        break;

                    case "textreader":
                        exceptions.Add("textreader");
                        break;

                    case "array":
                        exceptions.Add("array");
                        break;

                    case "icloneable":
                        exceptions.Add("str");
                        exceptions.Add("icloneable");
                        exceptions.Add("array");
                        break;
                        
                    default:
                        throw new Exception("Not all types got tested!");
                }

                #endregion

                foreach (var right in types)
                {
                    var expected = exceptions.Any(x => x == right.Key);
                    var actual = right.Value.IsAssignableTo(left.Value);

                    Assert.IsTrue(expected == actual, 
                        string.Format("Expected {0} {2}to be assignable to {1}.", right.Value.FullName, left.Value.FullName, expected ? "" : "not "));
                }
            }
        }

        public ILHelpersTests()
        {
            assembly = new AssemblyEmitter(CompilerArguments.Parse(new[] { "dummy.il" }));
        }
    }
}
