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
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return !isConst; }
        }

        private VariableWrapper variable;
        private bool isConst;
        public LocalVariableNode(SequencePoint point, VariableWrapper variable, bool isConst)
            : base(point)
        {
            this.variable = variable;
            this.isConst = isConst;
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
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return true; }
        }

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

    class FieldNode : SymbolNode, IFieldNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldReference Field { get { return field.FieldReference; } }
        public override TypeWrapper TypeWrapper { get { return field.TypeWrapper; } }
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

        private FieldWrapper field;
        public FieldNode(ExpressionNode instance, FieldWrapper field, Context parent, SequencePoint point)
            : base(field.Name, parent, point)
        {
            ObjectInstance = instance;
            //non-static field without an instance
            if(instance == null && !field.IsStatic)
            {
                //we're accessing a field from the same or derived class, insert "this"
                if(!parent.IsStaticContext() && parent.GetClass().TypeWrapper.IsAssignableTo(field.DeclaringType))
                {
                    ObjectInstance = new ThisNode(field.DeclaringType, point);
                }
                else
                {
                    throw new ParseException(point, "Cannot access non-static field {0} from a static context", field.Name);
                }
            }
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
    class PropertyNode : SymbolNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyReference Property { get; }
    }*/
}
