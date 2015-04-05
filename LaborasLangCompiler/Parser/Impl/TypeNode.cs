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
using Lexer;

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

        public new static TypeReference Parse(ContextNode parent, IAbstractSyntaxTree lexerNode)
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
            foreach (var node in lexerNode.Children)
            {
                builder.Append(node);
            }

            return builder.Type;
        }

        public static TypeNode Create(TypeReference type, ContextNode scope, SequencePoint point)
        {
            return new TypeNode(type, scope, point);
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }

        public class TypeBuilder
        {
            public TypeReference Type { get; private set; }

            private readonly Parser parser;
            private readonly ContextNode context;

            public TypeBuilder(ContextNode context)
            {
                this.parser = context.Parser;
                this.context = context;
            }

            public void Append(IAbstractSyntaxTree node)
            {
                if(Type == null)
                {
                    Type = TypeNode.Parse(context, node);
                }
                else
                {
                    var list = node.Children[0];
                    if (list.Type == Lexer.TokenType.FunctorParameters)
                    {
                        AppendFunctorTypeParams(list);
                    }
                    else if (list.Type == Lexer.TokenType.IndexNode)
                    {
                        AppendArrayParams(list);
                    }
                    else
                    {
                        ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(list), "Unexpected node type {0} in type", list.Type);
                    }
                }
            }

            private void AppendFunctorTypeParams(IAbstractSyntaxTree node)
            {
                var args = new List<TypeReference>();
                foreach (var param in node.Children)
                {
                    switch (param.Type)
                    {
                        case Lexer.TokenType.LeftParenthesis:
                        case Lexer.TokenType.RightParenthesis:
                        case Lexer.TokenType.Comma:
                            break;
                        case Lexer.TokenType.Type:
                            args.Add(Parse(context, param));
                            break;
                        default:
                            ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(param), "Unexpected node {0} while parsing functor type", param.Type);
                            break;//unreachable
                    }
                }

                if (args.Any(a => a.IsVoid()))
                        ErrorCode.IllegalMethodParam.ReportAndThrow(parser.GetSequencePoint(node), "Cannot declare method parameter of type void");

                Type = AssemblyRegistry.GetFunctorType(parser.Assembly, Type, args);
            }

            private void AppendArrayParams(IAbstractSyntaxTree node)
            {
                // [] has one dim
                int dims = 1;
                foreach(var subnode in node.Children)
                {
                    switch(subnode.Type)
                    {
                        case Lexer.TokenType.LeftBracket:
                        case Lexer.TokenType.RightBracket:
                            break;
                        case Lexer.TokenType.Comma:
                            dims++;
                            break;
                        default:
                            ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(subnode), "Unexpected node {0} while parsing array type", subnode.Type);
                            break;//unreachable
                    }
                }

                Type = AssemblyRegistry.GetArrayType(Type, dims);
            }

            public void Append(IEnumerable<TypeReference> paramz)
            {
                Type = AssemblyRegistry.GetFunctorType(parser.Assembly, Type, paramz.ToList());
            }
        }
    }
}