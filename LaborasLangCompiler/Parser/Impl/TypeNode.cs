using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class TypeNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return null; } }
        public TypeWrapper ParsedType { get; private set; }
        public TypeNode(TypeWrapper type, SequencePoint point)
            : base(point)
        {
            ParsedType = type;
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
                            break;
                        case Lexer.TokenType.Type:
                            args.Add(Parse(parser, parent, arg));
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(arg), "Unexpected node type, {0}", arg.Type);
                    }
                }

                ret = new FunctorTypeWrapper(parser.Assembly, ret, args);
            }

            return ret;
        }
    }
}