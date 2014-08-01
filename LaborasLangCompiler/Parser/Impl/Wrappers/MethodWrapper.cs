using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    interface MethodWrapper
    {
        MethodReference MethodReference { get; }
        TypeReference ResultType { get; }
        TypeReference MethodReturnType { get; }
        IEnumerable<TypeReference> ArgumentTypes { get; }
    }
}
