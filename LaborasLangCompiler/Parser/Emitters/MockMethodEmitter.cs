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
    class MockMethodEmitter : IMethodEmitter
    {
        public bool Parsed { get { return false; } }

        private readonly MethodEmitter method;

        public MethodReference Get()
        {
            return method.Get();
        }

        public void ParseTree(ICodeBlockNode tree)
        {
        }

        public void SetAsEntryPoint()
        {
            method.SetAsEntryPoint();
        }

        public ParameterDefinition AddArgument(ParameterDefinition parameter)
        {
            return method.AddArgument(parameter);
        }

        public MockMethodEmitter(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes)
        {
            method = new MethodEmitter(declaringType, name, returnType, methodAttributes);
        }
    }
}
