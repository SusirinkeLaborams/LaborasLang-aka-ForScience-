﻿using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class AssignmentOperatorNode : RValueNode, IAssignmentOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.AssignmentOperator; } }
        public override TypeReference ReturnType { get; set; }
        public ILValueNode LeftOperand { get; private set; }
        public IExpressionNode RightOperand { get; private set; }
        public static new AssignmentOperatorNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new AssignmentOperatorNode();
            var left = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]).ExtractLValue();
            var right = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            instance.ReturnType = left.ReturnType;

            var op = parser.ValueOf(lexerNode.Children[1]);
            if (op != "=")
                right = BinaryOperatorNode.Parse(parser, op.Remove(op.Length - 1), left, right);

            if (!right.ReturnType.IsAssignableTo(left.ReturnType))
                throw new TypeException(String.Format("Assigned {0} to {1}", instance.RightOperand.ReturnType, instance.LeftOperand.ReturnType));
            instance.RightOperand = right;
            instance.LeftOperand = left;
            return instance;    
        }
        public override string ToString()
        {
            return String.Format("(Assignment: {0} = {1})", LeftOperand, RightOperand);
        }
    }
}
