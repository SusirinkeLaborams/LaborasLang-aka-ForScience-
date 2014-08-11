using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalType : ExternalWrapperBase, TypeWrapper
    {
        public TypeReference TypeReference { get; private set; }

        public string FullName { get { return TypeReference.FullName; } }

        public ExternalType(AssemblyEmitter assembly, TypeReference type) : base(assembly)
        {
            TypeReference = type;
        }

        public TypeWrapper GetContainedType(string name)
        {
            var type = AssemblyRegistry.FindType(Assembly, TypeReference + "." + name);
            if (type != null)
                return new ExternalType(Assembly, type);
            else
                return null;
        }
        public FieldWrapper GetField(string name)
        {
            return new ExternalField(Assembly, AssemblyRegistry.GetField(Assembly, TypeReference, name));
        }
        public MethodWrapper GetMethod(string name)
        {
            return new ExternalMethod(Assembly, AssemblyRegistry.GetMethod(Assembly, TypeReference, name));
        }
        public IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(Assembly, TypeReference, name).Select(m => new ExternalMethod(Assembly, m));
        }
        public bool IsAssignableTo(TypeWrapper type)
        {
            if (!(type is ExternalType))
                return false;
            var t = ((ExternalType)type);
            return this.IsAssignableTo(type);
        }

        public static ExternalType GetFunctorType(AssemblyEmitter assembly, MethodReference method)
        {
            return new ExternalType(assembly, ILTools.AssemblyRegistry.GetFunctorType(assembly, method));
        }

        public bool IsNumericType()
        {
            return TypeReference.IsNumericType();
        }

        public bool IsStringType()
        {
            return TypeReference.IsStringType();
        }

        public bool IsIntegerType()
        {
            return TypeReference.IsIntegerType();
        }

        public bool IsBooleanType()
        {
            return TypeReference.IsBooleanType();
        }

        public override string ToString()
        {
            return TypeReference.ToString();
        }

        public bool IsFunctorType()
        {
            return TypeReference.IsFunctorType();
        }

        public int GetIntegerWidth()
        {
            return TypeReference.GetIntegerWidth();
        }

        public bool IsUnsignedInteger()
        {
            return TypeReference.IsUnsignedInteger();
        }
    }
}
