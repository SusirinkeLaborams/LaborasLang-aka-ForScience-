using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalField : FieldWrapper
    {
        public FieldReference FieldReference { get; private set; }
        public TypeReference ReturnType { get { return FieldReference.FieldType; } }
        public string Name { get { return FieldReference.Name; } }
        public ExternalField(FieldReference field)
        {
            this.FieldReference = field;
        }
    }
}
