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
        public override TypeReference ReturnType { get; set; }
        public LocalVariableNode(VariableDefinition variable, SequencePoint point)
            : base(point)
        {
            LocalVariable = variable;
            ReturnType = variable.VariableType;
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
        public override TypeReference ReturnType { get; set; }
        public FunctionArgumentNode(ParameterDefinition param, bool isFunctionStatic, SequencePoint point)
            : base(point)
        {
            Param = param;
            IsFunctionStatic = isFunctionStatic;
            ReturnType = param.ParameterType;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, Param.Name, ReturnType);
        }
    }

    class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; set; }
        public FieldReference Field { get; set; }
        public override TypeReference ReturnType { get; set; }
        public string Name { get; set; }
        public FieldNode(IExpressionNode instance, FieldReference field, SequencePoint point)
            : base(point)
        {
            ObjectInstance = instance;
            Field = field;
            ReturnType = field.FieldType;
        }
        public FieldNode(string name, TypeReference type, SequencePoint point)
            : base(point)
        {
            Name = name;
            ReturnType = type;
        }
        public override string ToString()
        {
            return String.Format("(LValueNode: {0} {1} {2})", LValueType, Field.Name, ReturnType);
        }
    }
    class FieldDeclarationNode : FieldNode
    {
        public IExpressionNode Initializer { get; set; }
        public FieldDeclarationNode(string name, TypeReference type, SequencePoint point)
            : base(name, type, point)
        {
        }
        public FieldReference CreateFieldDefinition(FieldAttributes attributes)
        {
            if (ReturnType != null)
                return Field = new FieldDefinition(Name, attributes, ReturnType);
            else
                throw new TypeException(SequencePoint, "Cannot create a field without a declared type");
        }
    }

    /*class PropertyNode : LValueNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyReference Property { get; }
    }*/
}
