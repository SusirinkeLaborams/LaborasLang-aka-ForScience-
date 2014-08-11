using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    interface TypeWrapper
    {
        TypeReference TypeReference { get; }
        string FullName { get; }
        FieldWrapper GetField(string name);
        TypeWrapper GetContainedType(string name);
        MethodWrapper GetMethod(string name);
        IEnumerable<MethodWrapper> GetMethods(string name);
        bool IsAssignableTo(TypeWrapper type);
        bool IsNumericType();
        bool IsStringType();
        bool IsIntegerType();
        bool IsBooleanType();
        bool IsFunctorType();
        bool IsUnsignedInteger();
        int GetIntegerWidth();
    }
}
