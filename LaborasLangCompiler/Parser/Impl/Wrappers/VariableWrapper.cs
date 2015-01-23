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
        public VariableDefinition VariableDefinition 
        { 
            get
            {
                if(definition == null)
                {
                    definition = new VariableDefinition(Name, TypeWrapper.TypeReference);
                }
                return definition;
            }
        }
        public TypeWrapper TypeWrapper { get; private set; }
        public string Name { get; private set; }

        private VariableDefinition definition;

        public VariableWrapper(string name, TypeWrapper parameterType)
        {
            Name = name;
            this.TypeWrapper = parameterType;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
