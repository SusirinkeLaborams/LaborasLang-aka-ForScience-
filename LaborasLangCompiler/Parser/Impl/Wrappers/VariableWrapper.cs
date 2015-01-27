using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class VariableWrapper
    {
        public VariableDefinition VariableDefinition { get { return definition.Value; } }
        public TypeReference TypeReference { get; private set; }
        public string Name { get; private set; }

        private Lazy<VariableDefinition> definition;

        public VariableWrapper(string name, TypeReference parameterType)
        {
            Name = name;
            this.TypeReference = parameterType;
            definition = new Lazy<VariableDefinition>(() => new VariableDefinition(Name, TypeReference));
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
