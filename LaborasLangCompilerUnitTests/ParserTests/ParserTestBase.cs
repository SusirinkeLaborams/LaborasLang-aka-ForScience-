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
using LaborasLangCompiler.Parser.Emitters;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    public class ParserTestBase
    {
        protected const string path = @"..\..\ParserTests\Trees\";

        protected static void CompareTrees(string source, [CallerMemberName] string name = "")
        {
            CompareTrees(source.Enumerate(), name.Enumerate(), name);
        }

        protected static void CompareTrees(string source, IEnumerable<ErrorCode> errors, [CallerMemberName] string name = "")
        {
            CompareTrees(source.Enumerate(), name.Enumerate(), errors, name);
        }

        protected static void CompareTrees(IEnumerable<string> sources, IEnumerable<string> names, [CallerMemberName] string name = "")
        {
            CompareTrees(sources, names, Enumerable.Empty<ErrorCode>(), name);
        }

        protected static void CompareTrees(IEnumerable<string> sources, IEnumerable<string> names, IEnumerable<ErrorCode> errors, [CallerMemberName] string name = "")
        {
            Errors.Clear();

            var compilerArgs = CompilerArguments.Parse(names.Select(n => n + ".ll").Union("/out:out.exe".Enumerate()).ToArray());
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
            var assembly = new AssemblyEmitter(compilerArgs);
            var file = path + name;

            var parser = ProjectParser.ParseAll(new MockEmitterSource(assembly), sources.ToArray(), names.ToArray());
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
