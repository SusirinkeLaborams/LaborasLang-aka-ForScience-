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

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode, ReturningNode
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get { return expression; } }
        public bool Returns { get { return true; } }

        private ExpressionNode expression;
        protected ReturnNode(SequencePoint point) : base(point) { }

        public static ReturnNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var returnType = parent.GetMethod().MethodReturnType;
            var instance = new ReturnNode(parser.GetSequencePoint(lexerNode));
            if (returnType.TypeEquals(parser.Void) && lexerNode.ChildrenCount != 2)
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, "Cannot return a value in a void method");
            }

            if (lexerNode.Children.Count == 3)
            {
                instance.expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[1], returnType);
                if (!instance.expression.ExpressionReturnType.IsAssignableTo(returnType) || !instance.expression.IsGettable)
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, "Method returns {0}, cannot return {1}", returnType, instance.Expression.ExpressionReturnType);
                }
            }
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Return:");
            builder.Indent(indent + 1).AppendLine("Expression:");
            builder.AppendLine(expression.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
