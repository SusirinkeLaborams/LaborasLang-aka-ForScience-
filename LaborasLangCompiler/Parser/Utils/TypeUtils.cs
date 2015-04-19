using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Utils
{
    static class TypeUtils
    {

        public static bool IsVoid(this TypeReference type)
        {
            return type.FullName == typeof(void).FullName;
        }

        [Pure]
        public static bool IsAuto(this TypeReference type)
        {
            return type is AutoType;
        }

        public static TypeReference GetNestedType(this TypeReference type, AssemblyEmitter assembly, string name)
        {
            return AssemblyRegistry.FindType(assembly, type.FullName + "." + name);
        }

        public static bool TypeEquals(this TypeReference left, TypeReference right)
        {
            return left.FullName == right.FullName;
        }

        public static bool IsAssignableTo(this IExpressionNode right, IExpressionNode left)
        {
            return right.ExpressionReturnType.IsAssignableTo(left.ExpressionReturnType);
        }

        public static void VerifyAccessible(MemberReference member, TypeReference scope, SequencePoint point)
        {
            if (!IsAccessbile(member, scope))
                ErrorCode.UnreachableMember.ReportAndThrow(point, "Member {0} is inaccessible from {1}", member, scope);
        }

        public static bool IsAccessbile(MemberReference member, TypeReference scope)
        {
            if (member is MethodReference)
            {
                return MetadataHelpers.IsAccessible((MethodReference)member, scope);
            }
            else if (member is TypeReference)
            {
                return MetadataHelpers.IsAccessible((TypeReference)member, scope);
            }
            else if (member is FieldReference)
            {
                return MetadataHelpers.IsAccessible((FieldReference)member, scope);
            }
            else if (member is PropertyReference)
            {
                var definition = ((PropertyReference)member).Resolve();
                var setter = definition.SetMethod;
                var getter = definition.GetMethod;
                return (setter != null && IsAccessbile(setter, scope)) || (getter != null && IsAccessbile(getter, scope));
            }
            else
            {
                throw new ArgumentException(String.Format("Unexpected member {0}", member.GetType().FullName));
            }
        }

        public static bool IsStatic(this MemberReference member)
        {
            if (member is FieldReference)
            {
                return ((FieldReference)member).Resolve().IsStatic;
            }
            else if (member is MethodReference)
            {
                return !((MethodReference)member).HasThis;
            }
            else if (member is TypeReference)
            {
                return true;
            }
            else if (member is PropertyReference)
            {
                var definition = ((PropertyReference)member).Resolve();

                var setter = definition.SetMethod;
                if (setter != null)
                    return setter.IsStatic;

                var getter = definition.GetMethod;
                if (getter != null)
                    return getter.IsStatic;
                throw new InvalidOperationException("Propery with no getter and no setter");
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static List<TypeReference> GetInheritance(this TypeReference type)
        {
            var ret = new List<TypeReference>();
            while (type != null)
            {
                ret.Add(type);
                type = type.GetBaseType();
            }
            ret.Reverse();
            return ret;
        }

        public static TypeReference GetCommonBaseClass(AssemblyEmitter assembly, IEnumerable<TypeReference> types)
        {
            if (types.Count() == 0)
                throw new ArgumentException("types must not be empty");

            var bases = types.Where(t => !t.IsTypeless()).Select(t => t.GetInheritance()).ToList();
            if(!bases.Any())
            {
                return NullType.Instance;
            }

            var result = assembly.TypeSystem.Object;

            for (int i = 0; i < bases.Min(x => x.Count); i++)
            {
                var first = bases[0][i];
                if(bases.All(x => x[i].FullName == first.FullName))
                {
                    result = first;
                }
            }

            return result;
        }

        public static IEnumerable<MethodReference> GetOperatorMethods(AssemblyEmitter assembly, IExpressionNode left, IExpressionNode right, string name)
        {
            return AssemblyRegistry.GetMethods(assembly, left.ExpressionReturnType, name)
                .Concat(AssemblyRegistry.GetMethods(assembly, right.ExpressionReturnType, name));
        }

        public static IEnumerable<MethodReference> GetOperatorMethods(AssemblyEmitter assembly, IExpressionNode operand, string name)
        {
            return AssemblyRegistry.GetMethods(assembly, operand.ExpressionReturnType, name);
        }
    }
}
