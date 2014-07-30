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
        public FieldReference Field { get; private set; }
        public TypeReference ReturnType { get { return Field.FieldType; } }
        public ExternalField(FieldReference field)
        {
            this.Field = field;
        }
    }
}
