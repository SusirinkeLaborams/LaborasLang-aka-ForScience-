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
    public class ParserTestBase
    {
        protected const string path = @"..\..\ParserTests\Trees\";
        private AssemblyEmitter assembly;

        protected ParserTestBase()
        {
            var compilerArgs = CompilerArguments.Parse(new[] { "ParserTests.il" });
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
            assembly = new AssemblyEmitter(compilerArgs);
        }

        protected void CompareTrees(string source, [CallerMemberName] string name = "")
        {
            CompareTrees(source.Enumerate(), name.Enumerate(), name);
        }

        protected void CompareTrees(string source, IEnumerable<ErrorCode> errors, [CallerMemberName] string name = "")
        {
            CompareTrees(source.Enumerate(), name.Enumerate(), errors, name);
        }

        protected void CompareTrees(IEnumerable<string> sources, IEnumerable<string> names, [CallerMemberName] string name = "")
        {
            CompareTrees(sources, names, Enumerable.Empty<ErrorCode>(), name);
        }

        protected void CompareTrees(IEnumerable<string> sources, IEnumerable<string> names, IEnumerable<ErrorCode> errors, [CallerMemberName] string name = "")
        {
            Errors.Clear();

            var file = path + name;

            names = names.Select(n => Path.Combine("$" + name, n));
            var parser = ProjectParser.ParseAll(assembly, sources.ToArray(), names.ToArray(), false);
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


            Assert.IsTrue(foundErrors.SetEquals(expectedErrors), "Errors: " + String.Join("\r\n", Errors.Reported));
            Assert.AreEqual(expected, result);
        }
    }
}
