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
        public ParameterDefinition ParameterDefinition { get { return definition.Value; } }
        public TypeWrapper TypeWrapper { get; private set; }
        public string Name { get; private set; }

        private ParameterAttributes attributes;
        private Lazy<ParameterDefinition> definition;

        public ParameterWrapper(string name, ParameterAttributes attributes, TypeWrapper parameterType)
        {
            this.Name = name;
            this.attributes = attributes;
            this.TypeWrapper = parameterType;
            definition = new Lazy<ParameterDefinition>(() => new ParameterDefinition(Name, this.attributes, TypeWrapper.TypeReference));
        }
    }
}
