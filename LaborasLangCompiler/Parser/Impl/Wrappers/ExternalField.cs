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
        public TypeWrapper ReturnType { get { return returnType; } }
        public string Name { get { return FieldReference.Name; } }

        private TypeWrapper returnType;
        public ExternalField(AssemblyEmitter assembly, FieldReference field) : base(assembly)
        {
            this.FieldReference = field;
            this.returnType = new ExternalType(assembly, field.FieldType);
        }
    }
}
