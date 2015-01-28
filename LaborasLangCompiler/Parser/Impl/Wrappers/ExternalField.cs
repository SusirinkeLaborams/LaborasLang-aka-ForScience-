using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    [Obsolete]
    class ExternalField : ExternalWrapperBase, FieldWrapper
    {
        public FieldReference FieldReference { get; private set; }
        public TypeReference TypeReference { get; private set; }
        public string Name { get { return FieldReference.Name; } }
        public bool IsStatic { get { return FieldReference.Resolve().IsStatic; } }
        public TypeReference DeclaringType { get; private set; }
        public MemberReference MemberReference { get { return FieldReference; } }

        public ExternalField(AssemblyEmitter assembly, FieldReference field) : base(assembly)
        {
            FieldReference = field;
            TypeReference = field.FieldType;
            DeclaringType = field.DeclaringType;
        }
    }
}
