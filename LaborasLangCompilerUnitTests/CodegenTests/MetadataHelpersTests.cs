using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.FrontEnd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    [TestClass]
    public class MetadataHelpersTests : TestBase
    {
        private AssemblyEmitter assembly;

        [TestMethod, TestCategory("Misc IL Tests")]
        public void TestIsAssignableTo()
        {
            var types = new Dictionary<string, TypeReference>();
            Action<string, string> addType = (tag, typeName) => types[tag] = AssemblyRegistry.FindType(assembly, typeName);

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
                        exceptions.Add("uint8");
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
                    var expected = exceptions.Any(exception => exception == right.Key);
                    var actual = right.Value.IsAssignableTo(left.Value);

                    Assert.IsTrue(expected == actual, 
                        string.Format("Expected {0} {2}to be assignable to {1}.", right.Value.FullName, left.Value.FullName, expected ? "" : "not "));
                }
            }
        }

        [TestMethod, TestCategory("Misc IL Tests")]
        public void TestIsEnumerable()
        {
            var stringType = AssemblyRegistry.FindType(assembly, "System.String");
            var intType = AssemblyRegistry.FindType(assembly, "System.Int32");
            var valueType = AssemblyRegistry.FindType(assembly, "System.ValueType");
            var boolType = AssemblyRegistry.FindType(assembly, "System.Boolean");
            var objType = AssemblyRegistry.FindType(assembly, "System.Object");

            Assert.IsTrue(stringType.IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(stringType, 1).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(stringType, 2).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(stringType, 3).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(stringType, 4).IsEnumerable());
            
            Assert.IsFalse(intType.IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(intType, 1).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(intType, 2).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(intType, 3).IsEnumerable());
            Assert.IsTrue(AssemblyRegistry.GetArrayType(intType, 4).IsEnumerable());

            Assert.IsFalse(valueType.IsEnumerable());
            Assert.IsFalse(boolType.IsEnumerable());
            Assert.IsFalse(objType.IsEnumerable());

            var objArray1d = AssemblyRegistry.GetArrayType(objType, 1);
            var objArray2d = AssemblyRegistry.GetArrayType(objType, 2);
            var objArrayJagged1d = AssemblyRegistry.GetArrayType(objArray1d, 1);
            var objArrayJagged2d = AssemblyRegistry.GetArrayType(objArray2d, 2);

            Assert.IsTrue(objArray1d.IsEnumerable());
            Assert.IsTrue(objArray2d.IsEnumerable());
            Assert.IsTrue(objArrayJagged1d.IsEnumerable());
            Assert.IsTrue(objArrayJagged2d.IsEnumerable());

            var listType = AssemblyRegistry.FindType(assembly, "System.Collections.Generic.List`1");
            Assert.IsTrue(listType.MakeGenericType(intType).IsEnumerable());
            Assert.IsTrue(listType.MakeGenericType(stringType).IsEnumerable());
        }

        [TestMethod, TestCategory("Misc IL Tests")]
        public void TestGetEnumerableElementType()
        {
            var stringType = AssemblyRegistry.FindType(assembly, "System.String");
            var intType = AssemblyRegistry.FindType(assembly, "System.Int32");
            var valueType = AssemblyRegistry.FindType(assembly, "System.ValueType");
            var boolType = AssemblyRegistry.FindType(assembly, "System.Boolean");
            var objType = AssemblyRegistry.FindType(assembly, "System.Object");

            Assert.AreEqual(stringType.GetEnumerableElementType().MetadataType, MetadataType.Char);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(stringType, 1).GetEnumerableElementType().MetadataType, MetadataType.String);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(stringType, 2).GetEnumerableElementType().MetadataType, MetadataType.String);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(stringType, 3).GetEnumerableElementType().MetadataType, MetadataType.String);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(stringType, 4).GetEnumerableElementType().MetadataType, MetadataType.String);

            Assert.AreEqual(intType.GetEnumerableElementType(), null);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(intType, 1).GetEnumerableElementType().MetadataType, MetadataType.Int32);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(intType, 2).GetEnumerableElementType().MetadataType, MetadataType.Int32);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(intType, 3).GetEnumerableElementType().MetadataType, MetadataType.Int32);
            Assert.AreEqual(AssemblyRegistry.GetArrayType(intType, 4).GetEnumerableElementType().MetadataType, MetadataType.Int32);

            Assert.AreEqual(valueType.GetEnumerableElementType(), null);
            Assert.AreEqual(boolType.GetEnumerableElementType(), null);
            Assert.AreEqual(objType.GetEnumerableElementType(), null);

            var objArray1d = AssemblyRegistry.GetArrayType(objType, 1);
            var objArray2d = AssemblyRegistry.GetArrayType(objType, 2);
            var objArrayJagged1d = AssemblyRegistry.GetArrayType(objArray1d, 1);
            var objArrayJagged2d = AssemblyRegistry.GetArrayType(objArray2d, 2);

            Assert.AreEqual(objArray1d.GetEnumerableElementType().MetadataType, MetadataType.Object);
            Assert.AreEqual(objArray2d.GetEnumerableElementType().MetadataType, MetadataType.Object);
            Assert.AreEqual(objArrayJagged1d.GetEnumerableElementType(), objArray1d);
            Assert.AreEqual(objArrayJagged2d.GetEnumerableElementType(), objArray2d);

            var listType = AssemblyRegistry.FindType(assembly, "System.Collections.Generic.List`1");
            Assert.AreEqual(listType.MakeGenericType(intType).GetEnumerableElementType().MetadataType, MetadataType.Int32);
            Assert.AreEqual(listType.MakeGenericType(stringType).GetEnumerableElementType().MetadataType, MetadataType.String);
        }

        public MetadataHelpersTests()
        {
            assembly = new AssemblyEmitter(CompilerArguments.Parse(new[] { "ExecuteTest.il" }));
        }
    }
}
