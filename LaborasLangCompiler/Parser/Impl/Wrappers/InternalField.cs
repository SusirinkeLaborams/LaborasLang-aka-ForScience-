using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class InternalField : FieldWrapper
    {
        public FieldReference FieldReference { get { return FieldDefinition; } }
        public FieldDefinition FieldDefinition { get; set; }
        public TypeReference ReturnType { get; set; }
        public string Name { get; private set; }
        public IExpressionNode Initializer { get; set; }
        public InternalField(TypeReference returnType, string name)
        {
            this.ReturnType = returnType;
            this.Name = name;
        }
    }
}
