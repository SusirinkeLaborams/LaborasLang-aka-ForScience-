using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    [Obsolete]
    interface MethodWrapper : MemberWrapper
    {
        MethodReference MethodReference { get; }
        TypeReference FunctorType { get; }
        TypeReference MethodReturnType { get; }
        IEnumerable<TypeReference> ParamTypes { get; }
    }
}
