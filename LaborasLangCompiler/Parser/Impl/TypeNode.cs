﻿using LaborasLangCompiler.Parser.Exceptions;
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
        public static TypeWrapper Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if(lexerNode.Type == Lexer.TokenType.FullSymbol)
                return DotOperatorNode.Parse(parser, parent, lexerNode).ExtractType();

            var ret = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]).ExtractType();

            if(lexerNode.ChildrenCount != 1)
            {
                var args = new List<TypeWrapper>();
                for(int i = 1; i < lexerNode.ChildrenCount; i++)
                {
                    var arg = lexerNode.Children[i];
                    switch(arg.Type)
                    {
                        case Lexer.TokenType.LeftBracket:
                        case Lexer.TokenType.RightBracket:
                            break;
                        case Lexer.TokenType.Type:
                            args.Add(Parse(parser, parent, arg));
                            break;
                        case Lexer.TokenType.TypeArgument:
                            if (arg.ChildrenCount > 2)
                                throw new ParseException(parser.GetSequencePoint(arg), "Method argument declaration instead of type");
                            else
                                args.Add(Parse(parser, parent, arg.Children[1]));
                            break;
                        case Lexer.TokenType.FullSymbol:
                            throw new ParseException(parser.GetSequencePoint(arg), "Method argument declaration instead of type");
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