﻿using LaborasLangCompiler.FrontEnd;
using Mono.Cecil;
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

            assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, compilerArgs.OutputPath, moduleParameters);
        }

        public void SetEntryPoint(MethodDefinition entryPoint)
        {
            assemblyDefinition.EntryPoint = entryPoint;
        }

        public void Save()
        {
            if (assemblyDefinition.EntryPoint == null && assemblyDefinition.MainModule.Kind != ModuleKind.Dll)
            {
                throw new Exception(string.Format("Current module kind ({0}) requires entry point set!", assemblyDefinition.MainModule.Kind)); 
            }

            assemblyDefinition.Write(assemblyDefinition.MainModule.Name);
        }
    }
}
