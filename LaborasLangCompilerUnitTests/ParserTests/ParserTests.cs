﻿using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompilerUnitTests.ILTests;
using LaborasLangCompilerUnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class ParserTests : TestBase
    {
        private const string path = @"..\..\ParserTests\SerializedLexerTrees\";
        private const bool lex = false;
        [TestMethod]
        public void FieldDeclarationTest()
        {
            string source = "auto a = 5; int b; int c = 10;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b, System.Int32 c = (Literal: System.Int32 10))";
            TestParser(source, expected, "FieldDeclarationTest", lex);
        }
        [TestMethod]
        public void ImplicitIntToLong()
        {
            string source = "auto a = 5; long b = a;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int64 b = (LValueNode: Field System.Int32))";
            TestParser(source, expected, "ImplicitIntToLong", lex);
        }
        [TestMethod]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Single 0))";
            try
            {
                TestParser(source, expected, "TypeExceptionTest", lex);
            }
            catch(TypeException)
            {
                //should throw
                return;
            }
            //no type error
            Assert.Fail();
        }
        [TestMethod]
        public void SomeTest()
        {
            string source = @"
                int a = 5;
                int b = a;
                auto Main = void(int arg)
                {
	                int a = 20;
	                a = 10;

	                auto f = int(int a, float b)
	                {
		                auto c = a * b;
	                };
                };";
            string expected = @"(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b = (LValueNode: Field System.Int32), $Functors.$System_Void$System_Int32 Main = (Function: $Functors.$System_Void$System_Int32(System.Int32 arg)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) arg, (LValueNode: LocalVariable System.Int32) a, (LValueNode: LocalVariable $Functors.$System_Int32$System_Int32$System_Single) f) Nodes: ((Declaration: (LValueNode: LocalVariable System.Int32) = (Literal: System.Int32 20)), (UnaryOp: RValue (Assignment: (LValueNode: LocalVariable System.Int32) = (Literal: System.Int32 10))), (Declaration: (LValueNode: LocalVariable $Functors.$System_Int32$System_Int32$System_Single) = (Function: $Functors.$System_Int32$System_Int32$System_Single(System.Int32 a, System.Single b)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) a, (LValueNode: FunctionArgument System.Single) b, (LValueNode: LocalVariable System.Single) c) Nodes: ((Declaration: (LValueNode: LocalVariable System.Single) = (BinaryOp: (LValueNode: FunctionArgument System.Int32) Multiplication (LValueNode: FunctionArgument System.Single)))))))))))";
            TestParser(source, expected, "SomeTest", lex);
        }
        [TestMethod]
        public void TestPrecedence()
        {
            string source = "auto a = 5 * 4 + 8 * (2 + 1);";
            string expected = "(ClassNode: Fields: System.Int32 a = (BinaryOp: (BinaryOp: (Literal: System.Int32 5) Multiplication (Literal: System.Int32 4)) Addition (BinaryOp: (Literal: System.Int32 8) Multiplication (BinaryOp: (Literal: System.Int32 2) Addition (Literal: System.Int32 1)))))";
            TestParser(source, expected, "TestPrecedence", lex);
        }
        private void TestParser(string source, string expected, string name, bool lex)
        {
            var compilerArgs = CompilerArguments.Parse(new[] { name + ".ll" });
            var assembly = new AssemblyEmitter(compilerArgs);
            var bytes = SourceReader.ReadSource(source);
            AstNode tree;
            if(lex)
            {
                tree = lexer.MakeTree(bytes);
                TreeSerializer.Serialize(path + name + ".xml", tree);
            }
            else
            {
                tree = TreeSerializer.Deserialize(path + name + ".xml");
            }
            Parser parser = new Parser(assembly, tree, bytes, "test", true);
            string result = parser.Root.ToString();
            Assert.AreEqual(expected, result);
        }
    }
}
