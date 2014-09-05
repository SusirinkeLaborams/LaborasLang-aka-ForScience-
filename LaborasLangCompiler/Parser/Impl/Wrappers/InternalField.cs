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
        public string Name { get; set; }
        public ExpressionNode Initializer { get; set; }
        public bool IsStatic { get; set; }

        public DeclarationInfo Declaration { get; private set; }
        public InternalField(DeclarationInfo declaration)
        {
            this.Declaration = declaration;
            this.IsStatic = true;
        }
    }
}
