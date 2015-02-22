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
    class FieldNode : MemberNode, IFieldNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }
        public FieldReference Field { get; private set; }
        public override TypeReference ExpressionReturnType { get { return Field.FieldType; } }
        public IExpressionNode ObjectInstance { get { return Instance; } }
        public override bool IsGettable
        {
            get
            {
                return true;
            }
        }
        public override bool IsSettable
        {
            get
            {
                return !Field.Resolve().Attributes.HasFlag(FieldAttributes.Literal | FieldAttributes.InitOnly);
            }
        }

        internal FieldNode(ExpressionNode instance, FieldReference field, ContextNode parent, SequencePoint point)
            : base(field, GetInstance(field, instance, parent, point), parent, point)
        {
            this.Field = field;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Field:");
            builder.Indent(indent + 1).AppendLine(Field.FullName);
            builder.Indent(indent + 1).AppendLine("Instance:");
            if (Instance == null)
                builder.Indent(indent + 2).Append("null");
            else
                builder.AppendLine(Instance.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
