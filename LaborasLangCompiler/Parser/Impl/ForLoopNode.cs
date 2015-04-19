using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using Lexer;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class ForLoopNode : ParserNode, IForLoopNode
    {
        public override NodeType Type
        {
            get { throw new NotImplementedException(); }
        }
        public ICodeBlockNode InitializationBlock { get { return initializer; } }
        public IExpressionNode ConditionBlock { get { return condition; } }
        public ICodeBlockNode IncrementBlock { get { return increment; } }
        public ICodeBlockNode Body { get { return body; } }

        private CodeBlockNode initializer;
        private ExpressionNode condition;
        private CodeBlockNode increment;
        private CodeBlockNode body;

        private ForLoopNode(CodeBlockNode init, ExpressionNode condition, CodeBlockNode increment, CodeBlockNode body, SequencePoint point)
            :base(point)
        {
            this.initializer = init;
            this.condition = condition;
            this.increment = increment;
            this.body = body;
        }

        public static ParserNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            if(lexerNode.Children.Any(node => node.Type == TokenType.In))
            {
                throw new NotImplementedException("foreach not implemented yet");
            }

            CodeBlockNode init = null;
            ExpressionNode condition = null;
            CodeBlockNode increment = null;

            IAbstractSyntaxTree initNode = lexerNode.Children[2];
            IAbstractSyntaxTree conditionNode = lexerNode.Children[4];
            IAbstractSyntaxTree incrementNode = lexerNode.Children[6];
            IAbstractSyntaxTree bodyNode = lexerNode.Children[8];

            if (initNode.Type != TokenType.Empty)
            {
                init = CodeBlockNode.Create(context, context.Parser.GetSequencePoint(initNode));
                init.AddStatement(initNode);
                //makes init scope encompass for scope
                context = init;
            }

            if (conditionNode.Type != TokenType.Empty)
            {
                condition = ExpressionNode.Parse(context, conditionNode, context.Parser.Bool);
            }

            if (incrementNode.Type != TokenType.Empty)
            {
                increment = CodeBlockNode.Create(context, context.Parser.GetSequencePoint(incrementNode));
                increment.AddStatement(incrementNode);
            }

            var body = CodeBlockNode.Parse(context, bodyNode);

            return ForLoopNode.Create(context, init, condition, increment, body, context.Parser.GetSequencePoint(lexerNode));
        }

        //context should be init block if init exists
        public static ForLoopNode Create(ContextNode context, CodeBlockNode init, ExpressionNode condition, CodeBlockNode increment, CodeBlockNode body, SequencePoint point)
        {
            if (init == null)
            {
                init = CodeBlockNode.Create(context, point);
            }

            if (condition == null)
            {
                condition = LiteralNode.Create(context, true, point);
            }

            if (increment == null)
            {
                increment = CodeBlockNode.Create(context, point);
            }

            if(!(condition.IsGettable && condition.ExpressionReturnType.IsAssignableTo(context.Parser.Bool)))
            {
                ErrorCode.InvalidCondition.ReportAndThrow(point, "Condition must be a gettable boolean expression");
            }

            return new ForLoopNode(init, condition, increment, body, point);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("For:");
            builder.Indent(indent + 1).AppendLine("Initializer:");
            builder.AppendLine(initializer.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Condition:");
            builder.AppendLine(condition.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Increment:");
            builder.AppendLine(increment.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Body:");
            builder.AppendLine(body.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
