using LaborasLangCompiler.Parser.Exceptions;
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
        public VariableDefinition LocalVariable { get { return variable.VariableDefinition; } }
        public override TypeWrapper TypeWrapper { get { return variable.TypeWrapper; } }
        public string Name {get { return LocalVariable.Name; } }

        private VariableWrapper variable;
        public LocalVariableNode(SequencePoint point, VariableWrapper variable)
            : base(point)
        {
            this.variable = variable;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Local:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            return builder.ToString();
        }
    }

    class FunctionArgumentNode : ExpressionNode, IMethodParamNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get { return parameter.ParameterDefinition; } }
        public bool IsMethodStatic { get; set; }
        public override TypeWrapper TypeWrapper { get { return parameter.TypeWrapper; } }
        public string Name { get { return Param.Name; } }

        private ParameterWrapper parameter;
        public FunctionArgumentNode(ParameterWrapper param, bool isFunctionStatic, SequencePoint point)
            : base(point)
        {
            this.parameter = param;
            IsMethodStatic = isFunctionStatic;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Param:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            return builder.ToString();
        }
    }

    class FieldNode : ExpressionNode, IFieldNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldReference Field { get { return field.FieldReference; } }
        public override TypeWrapper TypeWrapper { get { return field.TypeWrapper; } }
        public string Name { get { return Field.FullName; } }

        private FieldWrapper field;
        public FieldNode(IExpressionNode instance, FieldWrapper field, SequencePoint point)
            : base(point)
        {
            ObjectInstance = instance;
            this.field = field;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Field:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(field.Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            return builder.ToString();
        }
    }
    /*
    class PropertyNode : LValueNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyReference Property { get; }
    }*/
}
