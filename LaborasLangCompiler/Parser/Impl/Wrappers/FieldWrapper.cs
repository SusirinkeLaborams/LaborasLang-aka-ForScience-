using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    interface FieldWrapper
    {
        FieldReference FieldReference { get; }
        TypeWrapper TypeWrapper { get; }
        TypeWrapper DeclaringType { get; }
        string Name { get; }
        bool IsStatic { get; }
    }
}
