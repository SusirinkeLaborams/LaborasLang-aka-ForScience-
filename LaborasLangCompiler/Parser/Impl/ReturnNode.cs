using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using LaborasLangCompiler.Common;
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode, IReturningNode
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get { return expression; } }
        public bool Returns { get { return true; } }

        private ExpressionNode expression;
        private ReturnNode(SequencePoint point) : base(point) { }

        public static ReturnNode Parse(CodeBlockNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.ReturnNode);
            var point = context.Parser.GetSequencePoint(lexerNode);
            var returnType = context.GetMethod().MethodReturnType;
            ExpressionNode expression = null;

            if (lexerNode.Children.Count == 2)
            {
                expression = ExpressionNode.Parse(context, lexerNode.Children[1], returnType);
            }
            return Create(context, expression, point);
        }

        public static ReturnNode Create(CodeBlockNode context, ExpressionNode expression, SequencePoint point)
        {
            var instance = new ReturnNode(point);
            instance.expression = expression;
            var returnType = context.GetMethod().MethodReturnType;
            if(expression != null)
            {
                if(returnType.TypeEquals(context.Parser.Void))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, "Cannot return a value in a void method");
                }

                if (!expression.ExpressionReturnType.IsAssignableTo(returnType))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, "Method returns {0}, cannot return {1}", returnType, expression.ExpressionReturnType);
                }

                if (!expression.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(point, "Returned expression must be gettable");
                }
            }
            else
            {
                if(!returnType.TypeEquals(context.Parser.Void))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, "Method returns {0}, must return a value", returnType);
                }
            }

            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Return:");
            builder.Indent(indent + 1).AppendLine("Expression:");
            if (expression != null)
                builder.AppendLine(expression.ToString(indent + 2));
            else
                builder.Indent(indent + 2).AppendLine("none");
            return builder.ToString();
        }
    }
}
