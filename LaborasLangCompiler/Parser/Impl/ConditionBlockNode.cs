using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class ConditionBlockNode : ParserNode, IConditionBlock, ReturningNode
    {
        public override NodeType Type { get { return NodeType.ConditionBlock; } }
        public IExpressionNode Condition { get { return condition; } }
        public ICodeBlockNode TrueBlock { get { return trueBlock; } }
        public ICodeBlockNode FalseBlock { get { return falseBlock; } }
        protected ConditionBlockNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public bool Returns
        {
            get
            {
                return ((CodeBlockNode)TrueBlock).Returns && FalseBlock != null && ((CodeBlockNode)FalseBlock).Returns;
            }
        }

        private ExpressionNode condition;
        private CodeBlockNode trueBlock, falseBlock;
        public static ConditionBlockNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var point = parser.GetSequencePoint(lexerNode);
            var instance = new ConditionBlockNode(point);
            instance.condition = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            if (!instance.condition.ExpressionReturnType.IsAssignableTo(parser.Bool) || !instance.condition.IsGettable)
            {
                Errors.ReportAndThrow(ErrorCode.InvalidCondition, point, "Condition must be a gettable boolean expression");
            }
            instance.trueBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[4]);
            if (lexerNode.Children.Count > 5)
                instance.falseBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[6]);
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
