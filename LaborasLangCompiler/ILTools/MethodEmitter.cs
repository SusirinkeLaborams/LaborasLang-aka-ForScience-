using Mono.Cecil;
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

        public MethodEmitter(AssemblyRegistry assemblyRegistry, TypeDefinition declaringType, string name, TypeReference returnType, 
                                MethodAttributes methodAttributes = MethodAttributes.Private)
        {
            methodDefinition = new MethodDefinition(name, methodAttributes, returnType);
            declaringType.Methods.Add(methodDefinition);
        }

        void ParseTree(object/*WhateverTreeType*/ tree)
        {
            throw new NotImplementedException();
        }

        void EmitHelloWorld()
        {

        }
    }
}
