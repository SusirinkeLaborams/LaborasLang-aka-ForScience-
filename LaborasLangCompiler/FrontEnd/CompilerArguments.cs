using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaborasLangCompiler.FrontEnd
{
    internal class CompilerArguments
    {
        public readonly IReadOnlyList<string> SourceFiles;
        public readonly IReadOnlyList<string> References;
        public readonly IReadOnlyDictionary<string, string> FileToNamespaceMap;
        public readonly string OutputPath;
        public readonly ModuleKind ModuleKind;
        public readonly bool DebugBuild;

        public static CompilerArguments Parse(string[] args)
        {
            var sourceFiles = args.Where(arg => !arg.StartsWith("/", StringComparison.InvariantCultureIgnoreCase)).Distinct().ToArray();
            var references = args.Where(arg => arg.StartsWith("/ref:", StringComparison.InvariantCultureIgnoreCase));
            var outputPaths = args.Where(arg => arg.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase));
            var debugBuild = args.Any(arg => arg.Equals("/debug", StringComparison.InvariantCultureIgnoreCase));
            var moduleKinds = args.Where(arg => arg.Equals("/console", StringComparison.InvariantCultureIgnoreCase) ||
                                            arg.Equals("/windows", StringComparison.InvariantCultureIgnoreCase) ||
                                            arg.Equals("/dll", StringComparison.InvariantCultureIgnoreCase));
            var rootDirectories = args.Where(arg => arg.StartsWith("/root:", StringComparison.InvariantCultureIgnoreCase));

            var unknownOptions = args.Except(sourceFiles.Union(references).Union(outputPaths).Union(moduleKinds).Union(rootDirectories).Union(new string[] { "/debug" }));
            ParseUnknownOptions(unknownOptions);

            if (sourceFiles.Count() == 0)
            {
                throw new ArgumentException("No source files found to compile!");
            }

            var moduleKind = ParseModuleKinds(moduleKinds);
            var outputPath = ParseOutputPaths(outputPaths, sourceFiles, moduleKind);
            var referencesArray = references.Select(reference => reference.Substring(5))
                                   .Select(dllName => Path.Combine(ReferenceAssembliesPath, dllName))
                                   .Union(GetDefaultReferences())
                                   .ToArray();

            var fileToNamespaceMap = ParseRootDirectories(sourceFiles, rootDirectories.Select(dir => Path.GetFullPath(dir.Substring(6))).ToArray());

            return new CompilerArguments(sourceFiles, referencesArray, fileToNamespaceMap, outputPath, moduleKind, debugBuild);
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
                    case "/console":
                        return ModuleKind.Console;

                    case "/windows":
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

        private static Dictionary<string, string> ParseRootDirectories(string[] sourceFiles, string[] rootDirectories)
        {
            var fileToNamespaceMap = new Dictionary<string, string>();
            var typeFullNames = new Dictionary<string, List<string>>();
            bool namesClash = false;
            List<string> filesWithoutMatchingRoot = null;

            for (int i = 0; i < sourceFiles.Length; i++)
            {
                var filePath = Path.GetFullPath(sourceFiles[i]);
                int bestMatchingRootDirectory = -1;
                int bestMatchingRootDirectoryScore = -1;
                bool foundMatchingRootDirectory = false;

                for (int j = 0; j < rootDirectories.Length; j++)
                {
                    if (filePath.Length <= rootDirectories[j].Length)
                        continue;

                    int matchScore = 0;

                    while (matchScore < rootDirectories[j].Length && matchScore < filePath.Length && rootDirectories[j][matchScore] == filePath[matchScore])
                        matchScore++;

                    if (matchScore > 0)
                    {
                        foundMatchingRootDirectory = true;

                        if (matchScore > bestMatchingRootDirectoryScore)
                        {
                            bestMatchingRootDirectory = j;
                            bestMatchingRootDirectoryScore = matchScore;
                        }
                    }
                }

                string namespaze = null;

                if (foundMatchingRootDirectory)
                {
                    namespaze = Path.GetDirectoryName(filePath.Substring(bestMatchingRootDirectoryScore + 1)).Replace(Path.DirectorySeparatorChar, '.');
                }
                else if (rootDirectories.Length == 0)
                {
                    if (Path.IsPathRooted(sourceFiles[i]))
                    {
                        namespaze = string.Empty;
                    }
                    else
                    {
                        namespaze = Path.GetDirectoryName(sourceFiles[i]).Replace(Path.DirectorySeparatorChar, '.');
                    }
                }

                if (namespaze != null)
                {
                    var typeFullName = ((namespaze.Length > 0) ? namespaze + "." : string.Empty) + Path.GetFileNameWithoutExtension(sourceFiles[i]);

                    if (typeFullNames.ContainsKey(typeFullName))
                    {
                        namesClash = true;
                        typeFullNames[typeFullName].Add(sourceFiles[i]);
                    }
                    else
                    {
                        typeFullNames.Add(typeFullName, new List<string>() { sourceFiles[i] });
                        fileToNamespaceMap.Add(sourceFiles[i], namespaze);
                    }
                }
                else
                {
                    if (filesWithoutMatchingRoot == null)
                        filesWithoutMatchingRoot = new List<string>();

                    filesWithoutMatchingRoot.Add(sourceFiles[i]);
                }
            }

            var errorMessage = (filesWithoutMatchingRoot != null || namesClash) ? new StringBuilder() : null;

            if (filesWithoutMatchingRoot != null)
            {
                errorMessage.AppendLine("Error: Could not determine the namespace of the following files with given root directories:");

                foreach (var file in filesWithoutMatchingRoot)
                    errorMessage.AppendFormat("\t{0}{1}", file, Environment.NewLine);
            }

            if (namesClash)
            {
                foreach (var typeFullName in typeFullNames)
                {
                    if (typeFullName.Value.Count > 1)
                    {
                        errorMessage.AppendFormat("Error: Type name clash. The type of following files is \"{0}\":{1}", typeFullName.Key, Environment.NewLine);

                        foreach (var file in typeFullName.Value)
                        {
                            errorMessage.AppendFormat("\t{0}{1}", file, Environment.NewLine);
                        }
                    }
                }
            }

            if (errorMessage != null)                    
                throw new Exception(errorMessage.ToString());

            return fileToNamespaceMap;
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

        private static string referenceAssembliesPath;
        private static string ReferenceAssembliesPath
        {
            get
            {
                if (referenceAssembliesPath == null)
                {
                    referenceAssembliesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.5");
                }

                return referenceAssembliesPath;
            }
        }

        private static IEnumerable<string> GetDefaultReferences()
        {
            var references = new List<string>
            {
                Path.Combine(ReferenceAssembliesPath, "mscorlib.dll")
            };

            return references;
        }

        private CompilerArguments(IReadOnlyList<string> sourceFiles, IReadOnlyList<string> references, IReadOnlyDictionary<string, string> fileToNamespaceMap, string outputPath, 
            ModuleKind moduleKind, bool debugBuild)
        {
            SourceFiles = sourceFiles;
            References = references;
            FileToNamespaceMap = fileToNamespaceMap;
            OutputPath = outputPath;
            ModuleKind = moduleKind;
            DebugBuild = debugBuild;
        }
    }
}
