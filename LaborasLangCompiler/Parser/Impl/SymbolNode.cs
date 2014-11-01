﻿using LaborasLangCompiler.ILTools;
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
    class SymbolNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return null; } }
        public string Name { get; private set; }
        public TypeReference Scope { get; private set; }
        protected SymbolNode(string value, TypeReference scope, SequencePoint point)
            : base(point)
        {
            Name = value;
            Scope = scope;
        }
        public static new SymbolNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            return new SymbolNode(lexerNode.Content.ToString(), parent.GetClass().TypeReference, parser.GetSequencePoint(lexerNode));
        }
        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
    class NamespaceNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return null; } }
        public NamespaceWrapper Namespace { get; private set; }
        public NamespaceNode(NamespaceWrapper namespaze, SequencePoint point) : base(point)
        {
            this.Namespace = namespaze;
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
