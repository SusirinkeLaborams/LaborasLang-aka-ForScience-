//#define REWRITE
using LaborasLangCompiler.Common;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompilerUnitTests.CodegenTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.IO;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    public class ParserTestBase : TestBase
    {
        protected const string path = @"..\..\ParserTests\Trees\";
        private AssemblyEmitter assembly;

        protected ParserTestBase() :
            base(false)
        {
            Errors.Clear();

            var compilerArgs = CompilerArguments.Parse(new[] { "ParserTests.il" });
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
            assembly = new AssemblyEmitter(compilerArgs);
        }

        internal void CompareTrees(string source, [CallerMemberName] string testName = "")
        {
            CompareTrees(source.Enumerate(), testName.Enumerate(), testName);
        }

        internal void CompareTrees(string source, IEnumerable<ErrorCode> errors, [CallerMemberName] string testName = "")
        {
            CompareTrees(source.Enumerate(), testName.Enumerate(), errors, testName);
        }

        internal void CompareTrees(IEnumerable<string> sources, IEnumerable<string> fileNames, [CallerMemberName] string testName = "")
        {
            CompareTrees(sources, fileNames, Enumerable.Empty<ErrorCode>(), testName);
        }

        internal void CompareTrees(IEnumerable<string> sources, IEnumerable<string> fileNames, IEnumerable<ErrorCode> errors, [CallerMemberName] string testName = "")
        {
            Errors.Clear();

            var file = path + testName;
            var compilerArgs = CompilerArguments.Parse(fileNames.Select(n => Path.Combine("$" + testName, n)).Union(new[] { string.Format("/out:{0}.exe", testName) }).ToArray());
            var parser = ProjectParser.ParseAll(assembly, compilerArgs, sources.ToArray(), false);
            string result = parser.ToString();

            string expected = "";
#if REWRITE
            System.IO.File.WriteAllText(file, result);
            expected = result;
#else
            try
            {
                expected = System.IO.File.ReadAllText(file);
            }
            catch { }
#endif
            var foundErrors = Errors.Reported.Select(e => e.ErrorCode).ToHashSet();
            var expectedErrors = errors.ToHashSet();


            Assert.IsTrue(foundErrors.SetEquals(expectedErrors), "Errors: \r\n" + String.Join("\r\n", Errors.Reported));
            Assert.AreEqual(expected, result);
        }
    }
}
