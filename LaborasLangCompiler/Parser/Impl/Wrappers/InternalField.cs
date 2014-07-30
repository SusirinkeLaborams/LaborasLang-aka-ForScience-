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
        public FieldReference Field { get; set; }
        public TypeReference ReturnType { get; set; }
        public InternalField(TypeReference returnType)
        {
            this.ReturnType = returnType;
        }
    }
}
