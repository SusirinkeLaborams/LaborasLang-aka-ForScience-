using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    [Obsolete]
    interface FieldWrapper : MemberWrapper
    {
        FieldReference FieldReference { get; }
        TypeReference TypeReference { get; }
        string Name { get; }
    }
}
