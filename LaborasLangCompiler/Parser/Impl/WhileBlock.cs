using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Utils;
using Lexer;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class WhileBlock : ParserNode, IWhileBlockNode
    {
        public override NodeType Type { get { return NodeType.WhileBlock; } }
        public IExpressionNode Condition { get { return condition; } }
        public ICodeBlockNode ExecutedBlock { get { return block; } }

        private ExpressionNode condition;
        private CodeBlockNode block;
        protected WhileBlock(SequencePoint point) : base(point) { }

        public static WhileBlock Parse(ContextNode context, AbstractSyntaxTree lexerNode)
        {
            var point = context.Parser.GetSequencePoint(lexerNode);
            var condition = ExpressionNode.Parse(context, lexerNode.Children[2]);
            var block = CodeBlockNode.Parse(context, lexerNode.Children[4]);
            return Create(context, condition, block, point);
        }

        public static WhileBlock Create(ContextNode context, ExpressionNode condition, CodeBlockNode body, SequencePoint point)
        {
            var instance = new WhileBlock(point);
            if (!condition.ExpressionReturnType.TypeEquals(context.Parser.Bool) || !condition.IsGettable)
                ErrorCode.InvalidCondition.ReportAndThrow(point, "Condition must be a gettable boolean expression");
            instance.condition = condition;
            instance.block = body;
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("While:");
            builder.Indent(indent + 1).AppendLine("Condition:");
            builder.AppendLine(condition.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Block:");
            builder.AppendLine(block.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
