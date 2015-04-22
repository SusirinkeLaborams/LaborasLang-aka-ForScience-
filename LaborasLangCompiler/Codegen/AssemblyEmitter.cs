using LaborasLangCompiler.FrontEnd;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;

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

        public void Save()
        {
            if (assemblyDefinition.EntryPoint == null && assemblyDefinition.MainModule.Kind != ModuleKind.Dll)
            {
                throw new Exception(string.Format("Current module kind ({0}) requires entry point set!", assemblyDefinition.MainModule.Kind));
            }

            RemoveUnusedFunctors();

            var writerParams = new WriterParameters();
            writerParams.SymbolWriterProvider = new PdbWriterProvider();
            writerParams.WriteSymbols = true;

            assemblyDefinition.Write(outputPath, writerParams);
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
