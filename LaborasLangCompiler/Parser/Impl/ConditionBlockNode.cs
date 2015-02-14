﻿using LaborasLangCompiler.Parser.Utils;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class ConditionBlockNode : ParserNode, IConditionBlock, IReturningNode
    {
        public override NodeType Type { get { return NodeType.ConditionBlock; } }
        public IExpressionNode Condition { get { return condition; } }
        public ICodeBlockNode TrueBlock { get { return trueBlock; } }
        public ICodeBlockNode FalseBlock { get { return falseBlock; } }
        public bool Returns
        {
            get
            {
                return trueBlock.Returns && FalseBlock != null && falseBlock.Returns;
            }
        }

        private ExpressionNode condition;
        private CodeBlockNode trueBlock, falseBlock;

        private ConditionBlockNode(SequencePoint sequencePoint) : base(sequencePoint) { }

        public static ConditionBlockNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var point = parser.GetSequencePoint(lexerNode);
            var condition = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            var trueBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[4]);
            CodeBlockNode falseBlock = null;
            if (lexerNode.Children.Count > 5)
                falseBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[6]);
            return Create(parser, parent, condition, trueBlock, falseBlock, point);
        }

        public static ConditionBlockNode Create(Parser parser, Context parent, ExpressionNode condition, CodeBlockNode trueBlock, CodeBlockNode falseBlock, SequencePoint point)
        {
            var instance = new ConditionBlockNode(point);
            if (!condition.ExpressionReturnType.IsAssignableTo(parser.Bool) || !condition.IsGettable)
            {
                ErrorCode.InvalidCondition.ReportAndThrow(point, "Condition must be a gettable boolean expression");
            }
            instance.condition = condition;
            instance.trueBlock = trueBlock;
            instance.falseBlock = falseBlock;
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Condition:");
            builder.Indent(indent + 1).AppendLine("True:");
            builder.AppendLine(trueBlock.ToString(indent + 2));
            if (falseBlock != null)
            {
                builder.Indent(indent + 1).AppendLine("False:");
                builder.AppendLine(falseBlock.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
