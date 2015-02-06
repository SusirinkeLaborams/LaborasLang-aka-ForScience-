using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    static class Utils
    {
        public static bool IsFunctionDeclaration(this AstNode node)
        {
            if (node.Type == Lexer.TokenType.Function)
                return true;

            if (node.Type == Lexer.TokenType.Value)
                return node.Children[0].IsFunctionDeclaration();

            return false;
        }

        public static string GetSingleSymbolOrThrow(this AstNode node)
        {
            if (node.Type == Lexer.TokenType.Symbol)
                return node.Content.ToString();

            if (node.Type == Lexer.TokenType.FullSymbol && node.ChildrenCount == 1)
                return node.Children[0].Content.ToString();

            throw new InvalidOperationException("Node not a single symbol node");
        }

        public static StringBuilder Indent(this StringBuilder builder, int count)
        {
            builder.Append('\t', count);
            return builder;
        }

        public static bool IsVoid(this TypeReference type)
        {
            return type.FullName == typeof(void).FullName;
        }

        public static bool IsAuto(this TypeReference type)
        {
            return type is AutoType;
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }

        public static TypeReference GetNestedType(this TypeReference type, AssemblyEmitter assembly, string name)
        {
            return AssemblyRegistry.FindType(assembly, type.FullName + "." + name);
        }

        public static bool TypesEqual(TypeReference left, TypeReference right)
        {
            return left.FullName == right.FullName;
        }

        public static bool IsAssignableTo(this IExpressionNode right, IExpressionNode left)
        {
            return right.ExpressionReturnType.IsAssignableTo(left.ExpressionReturnType);
        }

        public static void VerifyAccessible(MemberReference member, TypeReference scope, SequencePoint point)
        {
            if(!IsAccessbile(member, scope))
                throw new TypeException(point, "Member {0} is inaccessible from {1}", member, scope);
        }

        public static bool IsAccessbile(MemberReference member, TypeReference scope)
        {
            if(member is MethodReference)
            {
                return ILHelpers.IsAccessible((MethodReference)member, scope);
            }
            else if(member is TypeReference)
            {
                return ILHelpers.IsAccessible((TypeReference)member, scope);
            }
            else if(member is FieldReference)
            {
                return ILHelpers.IsAccessible((FieldReference)member, scope);
            }
            else if(member is PropertyReference)
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
            if(member is FieldReference)
            {
                return ((FieldReference)member).Resolve().IsStatic;
            }else if(member is MethodReference)
            {
                return ((MethodReference)member).Resolve().IsStatic;
            }else if(member is TypeReference)
            {
                return true;
            }else if(member is PropertyReference)
            {
                var definition = ((PropertyReference)member).Resolve();

                var setter = definition.SetMethod;
                if (setter != null)
                    return setter.IsStatic;

                var getter = definition.GetMethod;
                if (getter != null)
                    return getter.IsStatic;
                throw new InvalidOperationException("Propery with no getter and no setter");
            }else
            {
                throw new ArgumentException();
            }
        }
    }
}
