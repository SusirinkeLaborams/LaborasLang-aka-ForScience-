using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalField : ExternalWrapperBase, FieldWrapper
    {
        public FieldReference FieldReference { get; private set; }
        public TypeWrapper TypeWrapper { get { return typeWrapper; } }
        public string Name { get { return FieldReference.Name; } }
        public bool IsStatic { get { return FieldReference.Resolve().IsStatic; } }

        private TypeWrapper typeWrapper;
        public ExternalField(AssemblyEmitter assembly, FieldReference field) : base(assembly)
        {
            this.FieldReference = field;
            this.typeWrapper = new ExternalType(assembly, field.FieldType);
        }
    }
}
