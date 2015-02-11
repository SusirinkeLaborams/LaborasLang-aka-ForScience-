using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    class LocalVariableNode : ExpressionNode, ILocalVariableNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; private set; }
        public override TypeReference ExpressionReturnType { get { return LocalVariable.VariableType; } }
        public string Name {get { return LocalVariable.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return !isConst; }
        }

        private bool isConst;
        public LocalVariableNode(SequencePoint point, VariableDefinition variable, bool isConst)
            : base(point)
        {
            this.LocalVariable = variable;
            this.isConst = isConst;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Local:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }

    class ParameterNode : ExpressionNode, IParameterNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.FunctionArgument; } }
        public ParameterDefinition Parameter { get; private set; }
        public override TypeReference ExpressionReturnType { get { return Parameter.ParameterType; } }
        public string Name { get { return Parameter.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return true; }
        }


        public ParameterNode(ParameterDefinition param, SequencePoint point)
            : base(point)
        {
            this.Parameter = param;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Param:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }

    class FieldNode : MemberNode, IFieldNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }
        public IExpressionNode ObjectInstance { get { return instance; } }
        public FieldReference Field { get; private set; }
        public override TypeReference ExpressionReturnType { get { return Field.FieldType; } }
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

        private ExpressionNode instance;
        public FieldNode(ExpressionNode instance, FieldReference field, Context parent, SequencePoint point)
            : base(field, parent, point)
        {
            this.instance = ThisNode.GetAccessingInstance(field, instance, parent, point);
            this.Field = field;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Field:");
            builder.Indent(indent + 1).AppendLine(Field.FullName);
            builder.Indent(indent + 1).AppendLine("Instance:");
            if (instance == null)
                builder.Indent(indent + 2).Append("null");
            else
                builder.AppendLine(instance.ToString(indent + 2));
            return builder.ToString();
        }
    }

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

        public IExpressionNode ObjectInstance { get { return instance; } }
        public PropertyReference Property { get; private set; }

        private ExpressionNode instance;
        private PropertyDefinition definition;

        public PropertyNode(ExpressionNode instance, PropertyReference property, Context scope, SequencePoint point)
            :base(property, scope, point)
        {
            this.instance = instance;
            this.Property = property;
            this.definition = property.Resolve();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Property:");
            builder.Indent(indent + 1).AppendLine(Property.FullName);
            builder.Indent(indent + 1).AppendLine("Instance:");
            if (instance == null)
                builder.Indent(indent + 2).Append("null");
            else
                builder.AppendLine(instance.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
