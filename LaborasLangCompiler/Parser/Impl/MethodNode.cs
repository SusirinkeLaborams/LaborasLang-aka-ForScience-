﻿using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodNode : RValueNode, IMethodNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeWrapper TypeWrapper { get { return method.FunctorType; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public MethodReference Method { get { return method.MethodReference; } }
        public MethodWrapper MethodWrapper { get { return method; } }

        private MethodWrapper method;

        public MethodNode(MethodWrapper method, IExpressionNode instance, SequencePoint point)
            : base(point)
        {
            this.method = method;
            this.ObjectInstance = instance;
        }
        public static MethodNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode, string name = null)
        {
            var method = FunctionDeclarationNode.Parse(parser, parent, lexerNode, name);
            return new MethodNode(method, null, method.SequencePoint);
        }
        public override string ToString()
        {
            return String.Format("(MethodNode: Instance: {0}, Method: {1})", ObjectInstance == null ? "null" : ObjectInstance.ToString(), method.MethodReference.Name);
        }
    }
}
