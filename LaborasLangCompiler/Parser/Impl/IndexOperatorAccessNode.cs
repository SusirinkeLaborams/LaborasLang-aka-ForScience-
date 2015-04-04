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
    class IndexOperatorAccessNode : ExpressionNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }

        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode Array { get { return array; } }

        public IReadOnlyList<IExpressionNode> Indices { get { return indices; } }

        public override bool IsSettable
        {
            get { return setter != null && TypeUtils.IsAccessbile(setter, context.GetClass().TypeReference); }
        }

        public override bool IsGettable
        {
            get { return getter != null && TypeUtils.IsAccessbile(getter, context.GetClass().TypeReference); }
        }

        private IReadOnlyList<ExpressionNode> indices;
        private ExpressionNode array;
        private TypeReference type;
        private MethodReference getter;
        private MethodReference setter;
        private ContextNode context;

        internal IndexOperatorAccessNode(ContextNode context, ExpressionNode array, TypeReference elementType, 
            MethodReference getter, MethodReference setter, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
            : base(point)
        {
            this.context = context;
            this.array = array;
            this.indices= indices;
            this.type = elementType;
            this.getter = getter;
            this.setter = setter;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("IndexOpAccess:");
            builder.Indent(indent + 1).AppendLine("Expression:");
            builder.Append(array.ToString(indent + 2)).AppendLine();
            builder.Indent(indent + 1).AppendFormat("ElementType: {0}", type.FullName).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Settable: {0}", setter != null).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Gettable: {0}", getter != null).AppendLine();
            builder.Indent(indent + 1).AppendLine("Indices:");
            foreach (var ind in indices)
            {
                builder.AppendLine(ind.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
