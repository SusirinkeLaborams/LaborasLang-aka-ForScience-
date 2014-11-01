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
        public override TypeWrapper TypeWrapper { get { return null; } }
        public TypeWrapper ParsedType { get; private set; }
        public override bool IsGettable { get { return true; } }
        public TypeNode(TypeWrapper type, TypeReference scope, SequencePoint point)
            : base(type != null ? type.FullName : null, scope, point)
        {
            ParsedType = type;
            if(type != null)
                Utils.VerifyAccessible(ParsedType.TypeReference, Scope, point);
        }
        public static new TypeWrapper Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            TypeNode node = null;
            if(lexerNode.Type == Lexer.TokenType.FullSymbol)
            {
                node = DotOperatorNode.Parse(parser, parent, lexerNode) as TypeNode;
                if (node != null)
                    return node.ParsedType;
                else
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Type expected");
            }

            TypeWrapper ret = null;
            node = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]) as TypeNode;
            if (node != null)
                ret = node.ParsedType;
            else
                throw new ParseException(parser.GetSequencePoint(lexerNode.Children[0]), "Type expected");

            if(lexerNode.ChildrenCount != 1)
            {
                var args = new List<TypeWrapper>();
                for(int i = 1; i < lexerNode.ChildrenCount; i++)
                {
                    var arg = lexerNode.Children[i];
                    switch(arg.Type)
                    {
                        case Lexer.TokenType.LeftParenthesis:
                        case Lexer.TokenType.RightParenthesis:
                        case Lexer.TokenType.Comma:
                            break;
                        case Lexer.TokenType.Type:
                            args.Add(Parse(parser, parent, arg));
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(arg), "Unexpected node type, {0}", arg.Type);
                    }
                }
                if (args.Any(a => a.FullName == parser.Void.FullName))
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Cannot declare method parameter of type void");

                ret = new FunctorTypeWrapper(parser.Assembly, ret, args);
            }

            return ret;
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}