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
        public VariableDefinition LocalVariable { get; set; }
        public override TypeReference ReturnType { get { return LocalVariable.VariableType; } }
        public LocalVariableNode(SequencePoint point, VariableDefinition variable)
            : base(point)
        {
            LocalVariable = variable;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, LocalVariable.Name, ReturnType);
        }
    }

    class FunctionArgumentNode : LValueNode, IFunctionArgumentNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; set; }
        public bool IsFunctionStatic { get; set; }
        public override TypeReference ReturnType { get { return Param.ParameterType; } }
        public FunctionArgumentNode(ParameterDefinition param, bool isFunctionStatic, SequencePoint point)
            : base(point)
        {
            Param = param;
            IsFunctionStatic = isFunctionStatic;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, Param.Name, ReturnType);
        }
    }

    class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldReference Field { get { return field.FieldReference; } }
        public override TypeReference ReturnType { get { return field.ReturnType; } }

        private FieldWrapper field;
        public FieldNode(IExpressionNode instance, FieldWrapper field, SequencePoint point)
            : base(point)
        {
            ObjectInstance = instance;
            this.field = field;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, Field.Name, ReturnType);
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
