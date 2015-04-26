using LaborasLangCompiler.Common;
using LaborasLangCompiler.FrontEnd;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace LaborasLangCompiler.Codegen
{
    internal class AssemblyEmitter
    {
        private readonly AssemblyDefinition assemblyDefinition;
        private readonly string outputPath;
        private readonly Dictionary<TypeReference, bool> functorUsageMap;

        public string OutputPath { get { return outputPath; } }
        public ModuleDefinition MainModule { get { return assemblyDefinition.MainModule; } }
        public TypeSystem TypeSystem { get { return assemblyDefinition.MainModule.TypeSystem; } }
        public bool DebugBuild { get; private set; }

        public AssemblyEmitter(CompilerArguments compilerArgs, Version version = null)
        {
            if (version == null)
            {
                version = new Version(0, 0, 0, 0);
            }

            var assemblyName = new AssemblyNameDefinition(Path.GetFileNameWithoutExtension(compilerArgs.OutputPath), version);
            var moduleParameters = new ModuleParameters()
            {
                Kind = compilerArgs.ModuleKind,
                Runtime = TargetRuntime.Net_4_0
            };

            assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, Path.GetFileName(compilerArgs.OutputPath), moduleParameters);
            AssemblyRegistry.RegisterAssembly(assemblyDefinition);
            outputPath = compilerArgs.OutputPath;
            DebugBuild = compilerArgs.DebugBuild;
            functorUsageMap = new Dictionary<TypeReference, bool>();
        }

        public void AddType(TypeDefinition type)
        {
            assemblyDefinition.MainModule.Types.Add(type);

            if (type.IsFunctorType())
                functorUsageMap.Add(type, false);
        }

        public void AddTypeUsage(TypeReference type)
        {
            var arrayType = type as ArrayType;

            if (arrayType != null)
            {
                AddTypeUsage(arrayType.ElementType);
                return;
            }

            if (type.IsFunctorType() && type.Resolve().Module == MainModule)
                functorUsageMap[type] = true;
        }

        public bool Save()
        {
            if (!FinalizeAssembly())
                return false;

            var writerParams = new WriterParameters();
            writerParams.SymbolWriterProvider = new PdbWriterProvider();
            writerParams.WriteSymbols = true;

            assemblyDefinition.Write(outputPath, writerParams);
            return true;
        }

        private bool FinalizeAssembly()
        {
            if (!CheckDuplicateMethods())
                return false;

            CheckThatEntryPointIsSet();
            RemoveUnusedFunctors();

            return true;
        }

        private bool CheckDuplicateMethods()
        {
            var types = MainModule.Types;
            List<List<MethodDefinition>> duplicateMethods = null;

            foreach (var type in types)
            {
                var sortedMethods = type.Methods.OrderBy(m => m.Name).ThenBy(m => m.Parameters.Count).ToArray();
                
                for (int i = 0; i < sortedMethods.Length - 1; i++)
                {
                    List<MethodDefinition> duplicates = null;

                    int j = i;
                    for (; j < sortedMethods.Length - 1 && AreMethodSignaturesIdentical(sortedMethods[j], sortedMethods[j + 1]); j++)
                    {
                        if (duplicates == null)
                        {
                            duplicates = new List<MethodDefinition>();
                            duplicates.Add(sortedMethods[j]);
                        }

                        duplicates.Add(sortedMethods[j + 1]);
                    }

                    i = j;

                    if (duplicates != null)
                    {
                        if (duplicateMethods == null)
                            duplicateMethods = new List<List<MethodDefinition>>();

                        duplicateMethods.Add(duplicates);
                    }
                }
            }

            if (duplicateMethods != null)
            {
                foreach (var duplicateMethodGroup in duplicateMethods)
                {
                    var builder = new StringBuilder("These methods have identical signatures:");
                    builder.Append(Environment.NewLine);

                    foreach (var method in duplicateMethodGroup)
                    {
                        builder.AppendFormat("\t{0}{1}{2}", Errors.SequencePointToString(method.Body.Instructions[0].SequencePoint), 
                            method.FullName, Environment.NewLine);
                    }

                    Errors.Report(ErrorCode.MethodAlreadyDeclared, builder.ToString());
                }

                return false;
            }

            return true;
        }

        private static bool AreMethodSignaturesIdentical(MethodDefinition m1, MethodDefinition m2)
        {
            if (m1.Name != m2.Name)
                return false;

            if (m1.Parameters.Count != m2.Parameters.Count)
                return false;

            var parameterCount = m1.Parameters.Count;

            for (int j = 0; j < parameterCount; j++)
            {
                if (m1.Parameters[j].ParameterType.FullName != m2.Parameters[j].ParameterType.FullName)
                    return false;
            }

            return true;
        }

        private void CheckThatEntryPointIsSet()
        {
            if (assemblyDefinition.EntryPoint == null && assemblyDefinition.MainModule.Kind != ModuleKind.Dll)
            {
                Errors.ReportAndThrow(ErrorCode.EntryPointNotSet, string.Format("Current module kind ({0}) requires entry point set!", assemblyDefinition.MainModule.Kind));
            }
        }

        private void RemoveUnusedFunctors()
        {
            foreach (var functor in functorUsageMap)
            {
                if (!functor.Value)
                    MainModule.Types.Remove(functor.Key.Resolve());
            }
        }
    }
}
