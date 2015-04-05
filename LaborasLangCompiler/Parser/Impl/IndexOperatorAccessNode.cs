using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class IndexOperatorAccessNode : PropertyNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.IndexOperator; } }

        public IReadOnlyList<IExpressionNode> Indices { get { return indices; } }

        private IReadOnlyList<ExpressionNode> indices;

        internal IndexOperatorAccessNode(ContextNode context, ExpressionNode instance, PropertyReference property, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
            : base(instance, property, context, point)
        {
            this.indices = indices;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("IndexOpAccess:");
            builder.Indent(indent + 1).AppendLine("Expression:");
            builder.Append(Instance.ToString(indent + 2)).AppendLine();
            builder.Indent(indent + 1).AppendFormat("ElementType: {0}", ExpressionReturnType.FullName).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Settable: {0}", IsSettable).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Gettable: {0}", IsGettable).AppendLine();
            builder.Indent(indent + 1).AppendLine("Indices:");
            foreach (var ind in indices)
            {
                builder.AppendLine(ind.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
