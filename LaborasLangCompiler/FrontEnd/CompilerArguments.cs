using Mono.Cecil;
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
        public readonly string OutputPath;
        public readonly ModuleKind ModuleKind;
        public readonly bool DebugBuild;

        public static CompilerArguments Parse(string[] args)
        {
            var sourceFiles = args.Where(x => !x.StartsWith("/", StringComparison.InvariantCultureIgnoreCase));
            var references = args.Where(x => x.StartsWith("/ref:", StringComparison.InvariantCultureIgnoreCase));
            var outputPaths = args.Where(x => x.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase));
            var debugBuild = args.Any(x => x.Equals("/debug", StringComparison.InvariantCultureIgnoreCase));
            var moduleKinds = args.Where(x => x.Equals("/console", StringComparison.InvariantCultureIgnoreCase) ||
                                            x.Equals("/windows", StringComparison.InvariantCultureIgnoreCase) ||
                                            x.Equals("/dll", StringComparison.InvariantCultureIgnoreCase));

            var unknownOptions = args.Except(sourceFiles.Union(references).Union(outputPaths).Union(moduleKinds));
            ParseUnknownOptions(unknownOptions);

            if (sourceFiles.Count() == 0)
            {
                throw new ArgumentException("No source files found to compile!");
            }

            var moduleKind = ParseModuleKinds(moduleKinds);
            var outputPath = ParseOutputPaths(outputPaths, sourceFiles, moduleKind);

            return new CompilerArguments(sourceFiles, references.Select(x => x.Substring(5)).Union(GetDefaultReferences()), outputPath,
                moduleKind, debugBuild);
        }

        private static void ParseUnknownOptions(IEnumerable<string> unknownOptions)
        {
            if (unknownOptions.Count() > 0)
            {
                var message = new StringBuilder();

                foreach (var option in unknownOptions)
                {
                    message.AppendLine(string.Format("Unknown compiler switch: {0}.", option));
                }

                throw new ArgumentException(message.ToString());
            }
        }

        private static ModuleKind ParseModuleKinds(IEnumerable<string> moduleKinds)
        {
            if (moduleKinds.Count() > 1)
            {
                throw new ArgumentException("More than one module kind specified!");
            }
            else if (moduleKinds.Count() == 0)
            {
                return ModuleKind.Console;
            }
            else
            {
                switch (moduleKinds.First().ToLowerInvariant())
                {
                    case "/console:":
                        return ModuleKind.Console;

                    case "/windows:":
                        return ModuleKind.Windows;     

                    case "/dll":
                        return ModuleKind.Dll;

                    default:
                        throw new ArgumentException(string.Format("Unknown module kind string: {0}.", moduleKinds.First()));
                }
            }
        }

        private static string ParseOutputPaths(IEnumerable<string> outputPaths, IEnumerable<string> sourceFiles, ModuleKind moduleKind)
        {
            string outputPath;
            var targetExtension = ModuleKindToExtension(moduleKind);

            if (outputPaths.Count() > 1)
            {
                throw new ArgumentException("More than one output path specified!");
            }
            else if (outputPaths.Count() == 0)
            {
                if (sourceFiles.Count() > 1)
                {
                    throw new ArgumentException("No output path and more than one source file specified!");
                }
                else
                {
                    outputPath = Path.ChangeExtension(sourceFiles.First(), targetExtension);
                }
            }
            else
            {
                outputPath = outputPaths.First().Substring(5);

                if (Path.GetExtension(outputPath) != targetExtension)
                {
                    throw new Exception(string.Join(Environment.NewLine, new string[]
                    { 
                        "Output path doesn't match specified module kind!",
                        string.Format("\tSpecified output path: \"{0}\"", outputPath),
                        string.Format("\tSpecified module kind: \"{0}\"", moduleKind)
                    }));                                          
                }
            }

            return outputPath;
        }

        private static string ModuleKindToExtension(ModuleKind moduleKind)
        {
            switch (moduleKind)
            {
                case ModuleKind.Console:
                case ModuleKind.Windows:
                    return ".exe";

                case ModuleKind.Dll:
                    return ".dll";

                default:
                    throw new ArgumentException(string.Format("Unknown module kind: {0}.", moduleKind), "moduleKind");
            }
        }

        private static IEnumerable<string> GetDefaultReferences()
        {
            var referenceAssembliesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.5");

            var references = new List<string>
            {
                Path.Combine(referenceAssembliesPath, "mscorlib.dll")
            };

            return references;
        }

        private CompilerArguments(IEnumerable<string> sourceFiles, IEnumerable<string> references, string outputPath, 
            ModuleKind moduleKind, bool debugBuild)
        {
            SourceFiles = sourceFiles.ToArray();
            References = references.ToArray();
            OutputPath = outputPath;
            ModuleKind = moduleKind;
            DebugBuild = debugBuild;
        }
    }
}
