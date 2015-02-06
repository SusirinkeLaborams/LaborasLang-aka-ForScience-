using LaborasLangCompiler.Common;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompilerUnitTests.ILTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    public class ParserTestBase : TestBase
    {
        protected const string path = @"..\..\ParserTests\Trees\";

        protected static void CanParse(string source, [CallerMemberName] string name = "")
        {
            CanParse(source, Enumerable.Empty<ErrorCode>(), name);
        }

        protected static void CanParse(string source, IEnumerable<ErrorCode> errors, [CallerMemberName] string name = "")
        {
            CanParse(new string[] { source }, new string[] { name }, errors);
        }

        protected static void CanParse(string[] sources, string[] names)
        {
            CanParse(sources, names, Enumerable.Empty<ErrorCode>());
        }

        protected static void CanParse(string[] sources, string[] names, IEnumerable<ErrorCode> errors)
        {
            ErrorHandling.Clear();

            var compilerArgs = CompilerArguments.Parse(names.Select(n => n + ".ll").Union("/out:out.exe".Yield()).ToArray());
            var assembly = new AssemblyEmitter(compilerArgs);
            ProjectParser.ParseAll(assembly, sources, names, false);

            var foundErrors = ErrorHandling.Errors.Select(e => e.ErrorCode).ToHashSet();
            var expectedErrors = errors.ToHashSet();

            Assert.IsTrue(foundErrors.SetEquals(expectedErrors));
        }


        protected static void CompareTrees(string source, [CallerMemberName] string name = "")
        {
            CompareTrees(source.Yield(), name.Yield(), name);
        }

        protected static void CompareTrees(IEnumerable<string> sources, IEnumerable<string> names, [CallerMemberName] string name = "")
        {
            ErrorHandling.Clear();

            var compilerArgs = CompilerArguments.Parse(names.Select(n => n + ".ll").Union("/out:out.exe".Yield()).ToArray());
            var assembly = new AssemblyEmitter(compilerArgs);
            var file = path + name;

            var parser = ProjectParser.ParseAll(assembly, sources.ToArray(), names.ToArray(), false);
            string result = parser.ToString();

#if REWRITE
            System.IO.File.WriteAllText(file, result);
#else

            string expected = "";
            try
            {
                expected = System.IO.File.ReadAllText(file);
            }
            catch { }
            Assert.AreEqual(expected, result);
#endif
            Assert.IsTrue(ErrorHandling.Errors.Count == 0);
        }
    }
}
