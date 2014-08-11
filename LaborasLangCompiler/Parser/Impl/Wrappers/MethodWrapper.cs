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
        TypeWrapper FunctorType { get; }
        TypeWrapper MethodReturnType { get; }
        IEnumerable<TypeWrapper> ArgumentTypes { get; }
    }
}
