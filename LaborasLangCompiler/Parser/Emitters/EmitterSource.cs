using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Emitters
{
    class EmitterSource : IEmitterSource
    {
        public AssemblyEmitter Assembly { get; private set; }

        public IMethodEmitter CreateMethod(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes)
        {
            return new MethodEmitter(declaringType, name, returnType, methodAttributes);
        }

        public EmitterSource(AssemblyEmitter assembly)
        {
            Assembly = assembly;
        }
    }
}
