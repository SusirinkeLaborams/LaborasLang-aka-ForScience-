using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class LValueNode : ExpressionNode, ILValueNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public abstract LValueNodeType LValueType { get; }
        protected LValueNode(SequencePoint point) : base(point) { }
    }

    class LocalVariableNode : LValueNode, ILocalVariableNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get { return variable.VariableDefinition; } }
        public override TypeWrapper TypeWrapper { get { return variable.TypeWrapper; } }

        private VariableWrapper variable;
        public LocalVariableNode(SequencePoint point, VariableWrapper variable)
            : base(point)
        {
            this.variable = variable;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, LocalVariable.Name, ExpressionReturnType);
        }
    }

    class FunctionArgumentNode : LValueNode, IMethodParamNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get { return parameter.ParameterDefinition; } }
        public bool IsMethodStatic { get; set; }
        public override TypeWrapper TypeWrapper { get { return parameter.TypeWrapper; } }

        private ParameterWrapper parameter;
        public FunctionArgumentNode(ParameterWrapper param, bool isFunctionStatic, SequencePoint point)
            : base(point)
        {
            this.parameter = param;
            IsMethodStatic = isFunctionStatic;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, Param.Name, ExpressionReturnType);
        }
    }

    class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldReference Field { get { return field.FieldReference; } }
        public override TypeWrapper TypeWrapper { get { return field.TypeWrapper; } }

        private FieldWrapper field;
        public FieldNode(IExpressionNode instance, FieldWrapper field, SequencePoint point)
            : base(point)
        {
            ObjectInstance = instance;
            this.field = field;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, field.Name, ExpressionReturnType);
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
