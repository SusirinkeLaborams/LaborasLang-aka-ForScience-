﻿using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolNode : IExpressionNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Intermediate; } }
        public TypeReference ReturnType { get { return null; } }
        public string Value { get; private set; }
        protected SymbolNode(string value)
        {
            Value = value;
        }
        public static SymbolNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            return new SymbolNode(parser.ValueOf(lexerNode));
        }
    }
    class NamespaceNode : SymbolNode
    {
        public NamespaceNode(string name) : base(name) { }
    }
    class SymbolCallNode : SymbolNode
    {
        public List<IExpressionNode> Arguments { get; private set; }
        protected SymbolCallNode(string name, List<IExpressionNode> args) : base(name)
        {
            Arguments = args;
        }
        public static new SymbolCallNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            string name = parser.ValueOf(lexerNode.Children[0]);
            var args = new List<IExpressionNode>();
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                args.Add(ExpressionNode.Parse(parser, parent, lexerNode.Children[i]));
            }
            return new SymbolCallNode(name, args);
        }
    }
    class TypeNode : IExpressionNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Intermediate; } }
        public TypeReference ReturnType { get { return null; } }
        public TypeReference ParsedType { get; private set; }
        public TypeNode(TypeReference type)
        {
            ParsedType = type;
        }
    }
}
