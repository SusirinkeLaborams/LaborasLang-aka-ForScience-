using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class TypeNode : SymbolNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public TypeReference ParsedType { get; private set; }
        public override bool IsGettable { get { return true; } }

        private TypeNode(TypeReference type, ContextNode scope, SequencePoint point)
            : base(type != null ? type.FullName : null, scope, point)
        {
            ParsedType = type;
            if(!type.IsAuto())
                TypeUtils.VerifyAccessible(ParsedType, Scope.GetClass().TypeReference, point);
        }

        public new static TypeReference Parse(ContextNode parent, AstNode lexerNode)
        {
            if (lexerNode.Type == Lexer.TokenType.FullSymbol)
            {
                TypeNode node = DotOperatorNode.Parse(parent, lexerNode) as TypeNode;
                if (node != null)
                    return node.ParsedType;
                else
                    ErrorCode.TypeExpected.ReportAndThrow(parent.Parser.GetSequencePoint(lexerNode), "Type expected");
            }

            TypeBuilder builder = new TypeBuilder(parent);
            foreach(AstNode node in lexerNode.Children)
            {
                builder.Append(node);
            }

            return builder.Type;
        }

        public static TypeNode Create(TypeReference type, ContextNode scope, SequencePoint point)
        {
            return new TypeNode(type, scope, point);
        }

        private static List<TypeReference> ParseArgumentList(ContextNode context, AstNode lexerNode)
        {
            var args = new List<TypeReference>();
            foreach(AstNode node in lexerNode.Children)
            {
                switch (node.Type)
                {
                    case Lexer.TokenType.LeftParenthesis:
                    case Lexer.TokenType.RightParenthesis:
                    case Lexer.TokenType.Comma:
                        break;
                    case Lexer.TokenType.Type:
                        args.Add(Parse(context, node));
                        break;
                    default:
                        ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(node), "Unexpected node {0} while parsing functor types", node.Type);
                        break;//unreachable
                }
            }
            return args;
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }

        public class TypeBuilder
        {
            public TypeReference Type { get; private set; }

            private readonly Parser parser;
            private readonly ContextNode parent;

            public TypeBuilder(ContextNode parent)
            {
                this.parser = parent.Parser;
                this.parent = parent;
            }

            public void Append(AstNode node)
            {
                if(Type == null)
                {
                    Type = TypeNode.Parse(parent, node);
                }
                else
                {
                    var args = ParseArgumentList(parent, node);
                    if (args.Any(a => a.IsVoid()))
                        ErrorCode.IllegalMethodParam.ReportAndThrow(parser.GetSequencePoint(node), "Cannot declare method parameter of type void");

                    Type = AssemblyRegistry.GetFunctorType(parser.Assembly, Type, args);
                }
            }

            public void Append(IEnumerable<TypeReference> paramz)
            {
                Type = AssemblyRegistry.GetFunctorType(parser.Assembly, Type, paramz.ToList());
            }
        }
    }
}