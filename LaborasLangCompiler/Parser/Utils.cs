using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
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

        public static void VerifyAccessible(MethodReference method, TypeReference scope, SequencePoint point)
        {
            if (!ILHelpers.IsAccessible(method, scope))
                throw new TypeException(point, "Method {0} is inaccessible from {1}", method, scope);
        }

        public static void VerifyAccessible(TypeReference type, TypeReference scope, SequencePoint point)
        {
            if (!ILHelpers.IsAccessible(type, scope))
                throw new TypeException(point, "Type {0} is inaccessible from {1}", type, scope);
        }

        public static void VerifyAccessible(FieldReference field, TypeReference scope, SequencePoint point)
        {
            if (!ILHelpers.IsAccessible(field, scope))
                throw new TypeException(point, "Field {0} is inaccessible from {1}", field, scope);
        }
    }
}
