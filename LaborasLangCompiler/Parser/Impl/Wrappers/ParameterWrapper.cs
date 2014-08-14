using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ParameterWrapper
    {
        public ParameterDefinition ParameterDefinition 
        { 
            get
            {
                if(definition == null)
                {
                    definition = new ParameterDefinition(Name, attributes, TypeWrapper.TypeReference);
                }
                return definition;
            }
        }
        public TypeWrapper TypeWrapper { get; private set; }
        public string Name { get; private set; }

        private ParameterAttributes attributes;
        private ParameterDefinition definition;

        public ParameterWrapper(string name, ParameterAttributes attributes, TypeWrapper parameterType)
        {
            this.Name = name;
            this.attributes = attributes;
            this.TypeWrapper = parameterType;
        }
    }
}
