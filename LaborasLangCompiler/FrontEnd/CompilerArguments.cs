using LaborasLangCompiler.Common;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public readonly bool HasErrors;

        public static CompilerArguments Parse(string[] args)
        {
            int errorCountBeforeParsingArgs = Errors.Reported.Count;

            IReadOnlyList<string> sourceFiles = args.Where(arg => !arg.StartsWith("/", StringComparison.InvariantCultureIgnoreCase)).Distinct().ToArray();
            var references = args.Where(arg => arg.StartsWith("/ref:", StringComparison.InvariantCultureIgnoreCase));
            var outputPaths = args.Where(arg => arg.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase));
            var debugBuild = args.Any(arg => arg.Equals("/debug", StringComparison.InvariantCultureIgnoreCase));
            var moduleKinds = args.Where(arg => arg.Equals("/console", StringComparison.InvariantCultureIgnoreCase) ||
                                            arg.Equals("/windows", StringComparison.InvariantCultureIgnoreCase) ||
                                            arg.Equals("/dll", StringComparison.InvariantCultureIgnoreCase));
            var rootDirectories = args.Where(arg => arg.StartsWith("/root:", StringComparison.InvariantCultureIgnoreCase));

            var unknownOptions = args.Except(sourceFiles.Union(references).Union(outputPaths).Union(moduleKinds).Union(rootDirectories).Union(new string[] { "/debug" }));
            ParseUnknownOptions(unknownOptions);

            CheckSourceFiles(ref sourceFiles);

            var moduleKind = ParseModuleKinds(moduleKinds);
            var outputPath = ParseOutputPaths(outputPaths, sourceFiles, moduleKind);
            var referencesArray = ParseReferences(references);

            var fileToNamespaceMap = ParseRootDirectories(sourceFiles, rootDirectories.Select(dir => Path.GetFullPath(dir.Substring(6))).ToArray());
            
            return new CompilerArguments(sourceFiles, referencesArray, fileToNamespaceMap, outputPath, moduleKind, debugBuild, Errors.Reported.Count - errorCountBeforeParsingArgs > 0);
        }

        private static void ParseUnknownOptions(IEnumerable<string> unknownOptions)
        {
            if (unknownOptions.Count() > 0)
            {
                var message = new StringBuilder();

                foreach (var option in unknownOptions)
                {
                    Errors.Report(ErrorCode.UnknownCompilerSwitch, string.Format("Unknown compiler switch: {0}.", option));
                }
            }
        }

        private static bool HasIllegalCharactersInPath(string path)
        {
            Contract.Requires(path != null);

            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];

                if (c == '\"' || c == '<' || c == '>' || c == '|' || c == '?' || c == '*' || c < 32)
                    return true;
            }

            return false;
        }

        private static void CheckSourceFiles(ref IReadOnlyList<string> sourceFiles)
        {
            if (sourceFiles.Count == 0)
            {
                Errors.Report(ErrorCode.NoSourceFiles, "No source files found to compile.");
                return;
            }

            bool ignoreMissingSourceFiles = Environment.GetEnvironmentVariable("LLC_IGNORE_NON_EXISTING_SOURCE_FILES") == "1";
            var checkedSourceFiles = new List<string>(sourceFiles.Count);

            for (int i = 0; i < sourceFiles.Count; i++)
            {
                if (HasIllegalCharactersInPath(sourceFiles[i]))
                {
                    Errors.Report(ErrorCode.IllegalCharactersInPath, string.Format("Illegal characters in path \"{0}\"", sourceFiles[i]));
                }
                else if (!ignoreMissingSourceFiles && !File.Exists(sourceFiles[i]))
                {
                    Errors.Report(ErrorCode.SourceFileDoesNotExist, string.Format("Source file \"{0}\" does not exist.", sourceFiles[i]));
                }
                else
                {
                    checkedSourceFiles.Add(sourceFiles[i]);
                }
            }

            sourceFiles = checkedSourceFiles;
        }

        private static ModuleKind ParseModuleKinds(IEnumerable<string> moduleKinds)
        {
            Contract.Ensures(Contract.Result<ModuleKind>() != ModuleKind.NetModule);
            var moduleKindCount = moduleKinds.Count();

            if (moduleKindCount > 1)
            {
                var errorMessage = new StringBuilder("More than one module kind specified:" + Environment.NewLine);

                foreach (var moduleKind in moduleKinds)
                {
                    errorMessage.AppendFormat("\t{0}{1}", moduleKind, Environment.NewLine);
                }

                Errors.Report(ErrorCode.MoreThanOneModuleKind, errorMessage.ToString());
                return default(ModuleKind);
            }
            else if (moduleKindCount == 0)
            {
                return ModuleKind.Console;
            }
            else
            {
                switch (moduleKinds.Single().ToLowerInvariant())
                {
                    case "/console":
                        return ModuleKind.Console;

                    case "/windows":
                        return ModuleKind.Windows;     

                    case "/dll":
                        return ModuleKind.Dll;

                    default:
                        ContractsHelper.AssumeUnreachable(string.Format("Unknown module kind: \"{0}\".", moduleKinds.First()));
                        return default(ModuleKind);
                }
            }
        }

        private static string ParseOutputPaths(IEnumerable<string> outputPaths, IReadOnlyList<string> sourceFiles, ModuleKind moduleKind)
        {
            string outputPath = null;
            var targetExtension = ModuleKindToExtension(moduleKind);
            var outputPathCount = outputPaths.Count();

            if (outputPathCount > 1)
            {
                Errors.Report(ErrorCode.MoreThanOneOutputPath, "More than one output path specified.");
                return null;
            }
            else if (outputPathCount == 0)
            {
                if (sourceFiles.Count > 1)
                {
                    Errors.Report(ErrorCode.NoOutputPaths, "No output path and more than one source file specified.");
                    return null;
                }
                else if (sourceFiles.Count == 1)
                {
                    outputPath = Path.ChangeExtension(sourceFiles.First(), targetExtension);
                }
                else
                {
                    return null;    // "No source files" error is already reported at this point
                }
            }
            else
            {
                outputPath = outputPaths.First().Substring(5);

                if (Path.GetFileNameWithoutExtension(outputPath) == Path.GetFileName(outputPath))
                {
                    outputPath = outputPath + targetExtension;
                }
                else if (Path.GetExtension(outputPath) != targetExtension)
                {
                    Errors.Report(ErrorCode.OutputPathAndModuleKindMismatch, string.Join(Environment.NewLine, new string[]
                    { 
                        "Output path doesn't match specified module kind!",
                        string.Format("\tSpecified output path: \"{0}\"", outputPath),
                        string.Format("\tSpecified module kind: \"{0}\"", moduleKind)
                    }));

                    return null;
                }
            }

            if (HasIllegalCharactersInPath(outputPath))
            {
                Errors.Report(ErrorCode.IllegalCharactersInPath, string.Format("Illegal characters in path \"{0}\"", outputPath));
            }

            return outputPath;
        }

        private static IReadOnlyList<string> ParseReferences(IEnumerable<string> references)
        {
            var parsedReferences = new List<string>();

            foreach (var reference in references.Select(reference => reference.Substring(5)))
            {
                if (HasIllegalCharactersInPath(reference))
                {
                    Errors.Report(ErrorCode.IllegalCharactersInPath, string.Format("Illegal characters in path \"{0}\"", reference));
                }
                else
                {
                    string referencePath = reference;
                    
                    if (!Path.IsPathRooted(reference))
                    {
                        if (!File.Exists(reference))
                        {
                            referencePath = Path.Combine(ReferenceAssembliesPath, reference);

                            if (!File.Exists(referencePath))
                            {
                                Errors.Report(ErrorCode.UnresolvedReference, 
                                    string.Format("Failed to resolve referenced assembly \"{0}\".{1}" +
                                        "\tConsidered \"{2}\", but the file could not be found.{1}" +
                                        "\tConsidered \"{3}\", but the file could not be found.", 
                                        reference, Environment.NewLine, Path.GetFullPath(reference), referencePath));
                                continue;
                            }
                        }
                    }
                    else if (!File.Exists(reference))
                    {
                        Errors.Report(ErrorCode.UnresolvedReference, string.Format("Failed to resolve referenced assembly \"{0}\".", reference));
                        continue;
                    }
                    else
                    {
                        referencePath = reference;
                    }

                    parsedReferences.Add(referencePath);
                }
            }

            foreach (var defaultReference in GetDefaultReferences())
            {
                bool shouldAddDefaultReference = true;

                foreach (var reference in parsedReferences)
                {
                    if (Path.GetFileNameWithoutExtension(reference) == Path.GetFileNameWithoutExtension(defaultReference))
                    {
                        shouldAddDefaultReference = false;
                    }
                }

                if (shouldAddDefaultReference)
                    parsedReferences.Add(defaultReference);
            }

            return parsedReferences;
        }

        private static Dictionary<string, string> ParseRootDirectories(IReadOnlyList<string> sourceFiles, string[] rootDirectories)
        {
            var fileToNamespaceMap = new Dictionary<string, string>();
            var typeFullNames = new Dictionary<string, List<string>>();
            bool namesClash = false;
            List<string> filesWithoutMatchingRoot = null;

            for (int i = 0; i < sourceFiles.Count; i++)
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

                    while (matchScore < rootDirectories[j].Length && rootDirectories[j][matchScore] == filePath[matchScore])
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
            
            if (filesWithoutMatchingRoot != null)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Error: Could not determine namespace of the following files with given root directories:");

                foreach (var file in filesWithoutMatchingRoot)
                    errorMessage.AppendFormat("\t{0}{1}", file, Environment.NewLine);

                Errors.Report(ErrorCode.UnspecifiedNamespace, errorMessage.ToString());
            }

            if (namesClash)
            {
                foreach (var typeFullName in typeFullNames)
                {
                    if (typeFullName.Value.Count > 1)
                    {
                        var errorMessage = new StringBuilder();
                        errorMessage.AppendFormat("Type name clash. The type of following files is \"{0}\":{1}", typeFullName.Key, Environment.NewLine);

                        foreach (var file in typeFullName.Value)
                        {
                            errorMessage.AppendFormat("\t{0}{1}", file, Environment.NewLine);
                        }

                        Errors.Report(ErrorCode.TypeNameClash, errorMessage.ToString());
                    }
                }
            }

            return fileToNamespaceMap;
        }

        private static string ModuleKindToExtension(ModuleKind moduleKind)
        {
            Contract.Requires(moduleKind != ModuleKind.NetModule);

            switch (moduleKind)
            {
                case ModuleKind.Console:
                case ModuleKind.Windows:
                    return ".exe";

                case ModuleKind.Dll:
                    return ".dll";

                default:
                    ContractsHelper.AssertUnreachable(string.Format("Unknown module kind: {0}.", moduleKind));
                    return string.Empty;
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
            ModuleKind moduleKind, bool debugBuild, bool hasErrors)
        {
            SourceFiles = sourceFiles;
            References = references;
            FileToNamespaceMap = fileToNamespaceMap;
            OutputPath = outputPath;
            ModuleKind = moduleKind;
            DebugBuild = debugBuild;
            HasErrors = hasErrors;
        }
    }
}
