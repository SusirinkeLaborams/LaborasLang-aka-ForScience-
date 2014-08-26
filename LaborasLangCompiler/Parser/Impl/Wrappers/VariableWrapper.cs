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
                    definition = new VariableDefinition(name, TypeWrapper.TypeReference);
                }
                return definition;
            }
        }
        public TypeWrapper TypeWrapper { get; private set; }

        private string name;
        private VariableDefinition definition;

        public VariableWrapper(string name, TypeWrapper parameterType)
        {
            this.name = name;
            this.TypeWrapper = parameterType;
        }
    }
}
