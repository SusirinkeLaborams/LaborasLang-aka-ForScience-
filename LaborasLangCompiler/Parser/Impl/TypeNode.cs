using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class TypeNode : SymbolNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return typeWrapper; } }
        public TypeWrapper ParsedType { get; private set; }
        public override bool IsGettable { get { return true; } }

        private TypeWrapper typeWrapper;
        public TypeNode(Parser parser, TypeWrapper type, TypeReference scope, SequencePoint point)
            : base(type != null ? type.FullName : null, scope, point)
        {
            typeWrapper = new ExternalType(parser.Assembly, typeof(Type));
            ParsedType = type;
            if(type != null)
                Utils.VerifyAccessible(ParsedType.TypeReference, Scope, point);
        }

        public static new TypeWrapper Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            if (lexerNode.Type == Lexer.TokenType.FullSymbol)
            {
                TypeNode node = DotOperatorNode.Parse(parser, parent, lexerNode) as TypeNode;
                if (node != null)
                    return node.ParsedType;
                else
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Type expected");
            }

            TypeBuilder builder = new TypeBuilder(parser, parent);
            foreach(AstNode node in lexerNode.Children)
            {
                builder.Append(node);
            }

            return builder.Type;
        }

        private static List<TypeWrapper> ParseArgumentList(Parser parser, Context parent, AstNode lexerNode)
        {
            var args = new List<TypeWrapper>();
            foreach(AstNode node in lexerNode.Children)
            {
                switch (node.Type)
                {
                    case Lexer.TokenType.LeftParenthesis:
                    case Lexer.TokenType.RightParenthesis:
                    case Lexer.TokenType.Comma:
                        break;
                    case Lexer.TokenType.Type:
                        args.Add(Parse(parser, parent, node));
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(node), "Unexpected node type, {0}", node.Type);
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
            public TypeWrapper Type { get; private set; }

            private Parser parser;
            private Context parent;

            public TypeBuilder(Parser parser, Context parent)
            {
                this.parser = parser;
                this.parent = parent;
            }

            public void Append(AstNode node)
            {
                if(Type == null)
                {
                    Type = TypeNode.Parse(parser, parent, node);
                }
                else
                {
                    var args = ParseArgumentList(parser, parent, node);
                    if(args.Any(a => a.IsVoid()))
                        throw new TypeException(parser.GetSequencePoint(node), "Cannot declare method parameter of type void");

                    Type = new FunctorTypeWrapper(parser.Assembly, Type, args);
                }
            }

            public void Append(IEnumerable<TypeWrapper> paramz)
            {
                Type = new FunctorTypeWrapper(parser.Assembly, Type, paramz);
            }
        }
    }
}