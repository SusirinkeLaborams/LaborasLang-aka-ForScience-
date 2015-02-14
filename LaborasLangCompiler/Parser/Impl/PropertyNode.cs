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
    class PropertyNode : MemberNode, IPropertyNode
    {
        public override ExpressionNodeType ExpressionType
        {
            get { return ExpressionNodeType.Property; }
        }

        public override TypeReference ExpressionReturnType
        {
            get { return IsGettable ? Property.PropertyType : null; }
        }

        public override bool IsSettable
        {
            get { return definition.SetMethod != null && TypeUtils.IsAccessbile(definition.SetMethod, Scope.GetClass().TypeReference); }
        }

        public override bool IsGettable
        {
            get { return definition.GetMethod != null && TypeUtils.IsAccessbile(definition.GetMethod, Scope.GetClass().TypeReference); }
        }

        public PropertyReference Property { get; private set; }

        private PropertyDefinition definition;

        internal PropertyNode(ExpressionNode instance, PropertyReference property, Context scope, SequencePoint point)
            : base(property, GetInstance(property, instance, scope, point), scope, point)
        {
            this.Property = property;
            this.definition = property.Resolve();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Property:");
            builder.Indent(indent + 1).AppendLine(Property.FullName);
            builder.Indent(indent + 1).AppendLine("Instance:");
            if (ObjectInstance == null)
                builder.Indent(indent + 2).Append("null");
            else
                builder.AppendLine(Instance.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
