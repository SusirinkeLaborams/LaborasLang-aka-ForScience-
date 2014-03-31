using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class MethodEmitter
    {
        MethodDefinition methodDefinition;
        ILProcessor ilProcessor;

        AssemblyRegistry assemblyRegistry;
        ModuleDefinition module;

        public MethodEmitter(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, string name, TypeReference returnType, 
                                MethodAttributes methodAttributes = MethodAttributes.Private)
        {
            methodDefinition = new MethodDefinition(name, methodAttributes, returnType);
            declaringType.AddMethod(methodDefinition);

            this.assemblyRegistry = assemblyRegistry;
            ilProcessor = methodDefinition.Body.GetILProcessor();
            module = declaringType.Module;

            if (module == null)
            {
                throw new ArgumentException("Declaring type isn't assigned to module!");
            }
        }

        public void EmitHelloWorld()
        {
            var console = assemblyRegistry.GetType("System.Console");

            var consoleWriteLine = module.Import(assemblyRegistry.GetMethods(console, "WriteLine")
                           .Where(x => x.Parameters.Count == 1 && x.Parameters[0].ParameterType.FullName == "System.String").Single());
            var consoleReadLine = module.Import(assemblyRegistry.GetMethods(console, "ReadLine").Where(x => x.Parameters.Count == 0).Single());

            Ldstr("Hello, world!");
            Call(consoleWriteLine);
            Call(consoleReadLine);
            Pop();
            Ret();
        }

        public void ParseTree(object/*WhateverTreeType*/ tree)
        {
            throw new NotImplementedException();
        }

        public void SetAsEntryPoint()
        {
            if ((methodDefinition.Attributes & MethodAttributes.Static) == 0)
            {
                throw new Exception("Entry point must be static!");
            }

            methodDefinition.DeclaringType.Module.EntryPoint = methodDefinition;
        }

        private void Ldstr(string str)
        {
            ilProcessor.Emit(OpCodes.Ldstr, str);
        }

        private void Call(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Call, method);
        }

        private void Ret()
        {
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void Pop()
        {
            ilProcessor.Emit(OpCodes.Pop);
        }
    }
}
