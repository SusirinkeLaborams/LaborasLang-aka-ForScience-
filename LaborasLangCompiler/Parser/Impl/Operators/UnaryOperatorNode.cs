﻿using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperatorNode : RValueNode, IUnaryOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.UnaryOperator; } }
        public override TypeReference ReturnType  { get; set; }
        public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
        public IExpressionNode Operand { get; private set; }
        private UnaryOperatorNode(UnaryOperatorNodeType type, IExpressionNode operand)
        {
            Operand = operand;
            UnaryOperatorType = type;
        }
        public static ExpressionNode Parser(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            throw new NotImplementedException();
        }
        public static UnaryOperatorNode Void(ExpressionNode expression)
        {
            return new UnaryOperatorNode(UnaryOperatorNodeType.VoidOperator, expression);
        }
    }
}