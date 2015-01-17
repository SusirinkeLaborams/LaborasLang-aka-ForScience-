using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    abstract class TypeWrapper : ExternalWrapperBase, MemberWrapper
    {
        public abstract TypeReference TypeReference { get; }
        public abstract string FullName { get; }
        public abstract TypeWrapper FunctorReturnType { get; }
        public abstract IEnumerable<TypeWrapper> FunctorParamTypes { get; }
        public abstract FieldWrapper GetField(string name);
        public abstract TypeWrapper GetContainedType(string name);
        public abstract MethodWrapper GetMethod(string name);
        public abstract IEnumerable<MethodWrapper> GetMethods(string name);
        public abstract TypeWrapper DeclaringType { get; }
        public MemberReference MemberReference { get { return TypeReference; } }
        public bool IsStatic { get { return true; } }

        public TypeWrapper(AssemblyEmitter assembly) : base(assembly) { }

        public bool IsAssignableTo(TypeWrapper type)
        {
            return TypeReference.IsAssignableTo(type.TypeReference);
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
        public virtual bool IsFunctorType()
        {
            return TypeReference.IsFunctorType();
        }
        public bool IsUnsignedInteger()
        {
            return TypeReference.IsUnsignedInteger();
        }
        public int GetIntegerWidth()
        {
            return TypeReference.GetIntegerWidth();
        }
        public bool MatchesArgumentList(IEnumerable<TypeWrapper> args)
        {
            if (!IsFunctorType())
                return false;
            else
                return ILHelpers.MatchesArgumentList(TypeReference, args.Select(arg => arg.TypeReference).ToList());
        }
        public override string ToString()
        {
            return FullName;
        }
    }
}
