using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class ValueCreationNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ValueCreation; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }
        public override TypeReference ExpressionReturnType { get { return type; } }

        private readonly TypeReference type;

        internal ValueCreationNode(TypeReference type, SequencePoint point) : base(point)
        {
            this.type = type;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ValueCreation:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", type).AppendLine();
            return builder.ToString();
        }
    }
}
