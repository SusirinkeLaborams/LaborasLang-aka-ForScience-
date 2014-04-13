﻿using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ReturnType { get; }
        public static new ExpressionNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            switch (lexerNode.Token.Name)
            {
                case "Symbol":
                    return LValueNode.Parse(parser, parent, lexerNode);
                case "Literal":
                    return LiteralNode.Parse(parser, parent, lexerNode);
                case "Value":
                    return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                case "Sum":
                    goto case "Product";
                case "Product":
                    return BinaryOperatorNode.Parse(parser, parent, lexerNode);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
