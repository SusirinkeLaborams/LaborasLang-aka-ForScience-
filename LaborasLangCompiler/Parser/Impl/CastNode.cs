using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
using Lexer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class CastNode : ExpressionNode, ICastNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Cast; } }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }
        public IExpressionNode TargetExpression { get { return target; } }

        private ExpressionNode target;
        private TypeReference type;

        private CastNode(TypeReference type, ExpressionNode target, SequencePoint point)
            :base(point)
        {
            this.type = type;
            this.target = target;
        }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.PrefixNode);
            Contract.Requires(lexerNode.Children[1].Type == Lexer.TokenType.CastOperator);

            var type = TypeNode.Parse(context, lexerNode.Children[1].Children[1]);
            var target = ExpressionNode.Parse(context, lexerNode.Children[0], type);

            if(!target.IsGettable)
            {
                ErrorCode.NotAnRValue.ReportAndThrow(target.SequencePoint, "The expression being cast must be gettable");
            }

            if(!target.ExpressionReturnType.IsCastableTo(type))
            {
                ErrorCode.IllegalCast.ReportAndThrow(context.Parser.GetSequencePoint(lexerNode), "Cannot cast expression of type {0} to {1}", target.ExpressionReturnType, type);
            }

            return new CastNode(type, target, context.Parser.GetSequencePoint(lexerNode));
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Cast:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", type).AppendLine();
            builder.Indent(indent + 1).AppendLine("Expression:");
            builder.AppendLine(target.ToString(indent + 2));
            return builder.ToString();
        }
    }
}