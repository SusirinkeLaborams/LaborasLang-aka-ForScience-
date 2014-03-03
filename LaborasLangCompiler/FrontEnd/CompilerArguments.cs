using LaborasLangCompiler.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.FrontEnd
{
    internal class CompilerArguments
    {
        public readonly string[] SourceFiles;
        public readonly string[] References;

        public static CompilerArguments Parse(string[] args)
        {
            var sourceFiles = args.Where(x => !x.StartsWith("/"));
            var references = args.Where(x => x.StartsWith("/ref:"));

            var unknownOptions = args.Except(sourceFiles.Union(references));

            if (unknownOptions.Count() > 0)
            {
                var message = new StringBuilder();

                foreach (var option in unknownOptions)
                {
                    message.AppendLine(string.Format("Unknown compiler switch: {0}.", option));
                }

                throw new ArgumentException(message.ToString());
            }

            return new CompilerArguments(sourceFiles, references.Select(x => x.Substring(5)).Union(GetDefaultReferences()));
        }

        private static IEnumerable<string> GetDefaultReferences()
        {
            var referenceAssembliesPath = Utils.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.5");

            var references = new List<string>
            {
                Path.Combine(referenceAssembliesPath, "mscorlib.dll")
            };

            return references;
        }

        private CompilerArguments(IEnumerable<string> sourceFiles, IEnumerable<string> references)
        {
            SourceFiles = sourceFiles.ToArray();
            References = references.ToArray();

            if (SourceFiles.Length == 0)
            {
                throw new ArgumentException("No source files found to compile!");
            }
        }
    }
}
