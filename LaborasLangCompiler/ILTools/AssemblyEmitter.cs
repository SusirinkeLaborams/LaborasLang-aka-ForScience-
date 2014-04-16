using LaborasLangCompiler.FrontEnd;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class AssemblyEmitter
    {
        private AssemblyDefinition assemblyDefinition;
        private readonly string outputPath;

        public string OutputPath { get { return outputPath; } }
        public ModuleDefinition MainModule { get { return assemblyDefinition.MainModule; } }

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
        }

        public void AddType(TypeDefinition type)
        {
            assemblyDefinition.MainModule.Types.Add(type);
        }

        public TypeReference TypeToTypeReference(Type type)
        {
            return assemblyDefinition.MainModule.Import(type);
        }

        public void Save()
        {
            if (assemblyDefinition.EntryPoint == null && assemblyDefinition.MainModule.Kind != ModuleKind.Dll)
            {
                throw new Exception(string.Format("Current module kind ({0}) requires entry point set!", assemblyDefinition.MainModule.Kind));
            }

            var writerParams = new WriterParameters();
            writerParams.SymbolWriterProvider = new PdbWriterProvider();
            writerParams.WriteSymbols = true;

            assemblyDefinition.Write(outputPath, writerParams);
        }
    }
}
