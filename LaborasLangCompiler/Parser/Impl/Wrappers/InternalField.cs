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
        public TypeWrapper TypeWrapper { get; set; }
        public string Name { get; private set; }
        public ExpressionNode Initializer { get; set; }
        public bool IsStatic { get; private set; }
        public InternalField(TypeWrapper returnType, string name, ExpressionNode init = null)
        {
            this.TypeWrapper = returnType;
            this.Name = name;
            this.Initializer = init;
            this.IsStatic = true;
        }
    }
}
