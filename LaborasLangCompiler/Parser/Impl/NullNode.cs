using LaborasLangCompiler.Common;
using Lexer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class NullNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType
        {
            get { return ExpressionNodeType.Null; }
        }

        public override TypeReference ExpressionReturnType
        {
            get { return NullType.Instance; }
        }

        public override bool IsGettable
        {
            get { return true; }
        }

        public override bool IsSettable
        {
            get { return false; }
        }

        private NullNode(SequencePoint point) : base(point) { }

        public static NullNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.Null);
            return NullNode.Create(context.Parser.GetSequencePoint(lexerNode));
        }

        public static NullNode Create(SequencePoint point)
        {
            return new NullNode(point);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("NullNode");
            return builder.ToString();
        }
    }
}
