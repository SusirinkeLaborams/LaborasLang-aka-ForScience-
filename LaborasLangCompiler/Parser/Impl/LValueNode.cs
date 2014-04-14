using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class LValueNode : ExpressionNode, ILValueNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public abstract LValueNodeType LValueType { get; }
        public static new LValueNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            if(lexerNode.Token.Name == "Symbol")
            {
                var value = parser.GetNodeValue(lexerNode);
                LValueNode instance = null;
                if(parentBlock != null)
                    instance = parentBlock.GetSymbol(value);
                if(instance == null)
                    instance = parentClass.GetField(value);
                if (instance != null)
                    return instance;
                else
                    throw new SymbolNotFoundException("Symbol " + value + " not found");
            }
            throw new NotImplementedException();
        }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is LValueNode))
                return false;
            var that = (LValueNode)obj;

            return base.Equals(obj) && LValueType == that.LValueType;
        }
    }

    class LocalVariableNode : LValueNode, ILocalVariableNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; set; }
        public override TypeReference ReturnType { get; set; }
        public LocalVariableNode(VariableDefinition variable)
        {
            LocalVariable = variable;
            ReturnType = variable.VariableType;
        }
    }

    class FunctionArgumentNode : LValueNode, IFunctionArgumentNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; set; }
        public override TypeReference ReturnType { get; set; }
        public FunctionArgumentNode(ParameterDefinition param)
        {
            Param = param;
            ReturnType = param.ParameterType;
        }
    }

    class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; set; }
        public FieldDefinition Field { get; set; }
        public override TypeReference ReturnType { get; set; }
        public string Name { get; set; }
        public FieldNode(ExpressionNode instance, FieldDefinition field)
        {
            ObjectInstance = instance;
            Field = field;
            ReturnType = field.FieldType;
        }
        public FieldNode(string name, TypeReference type)
        {
            Name = name;
            ReturnType = type;
        }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is FieldNode))
                return false;
            var that = (FieldNode)obj;
            if (ObjectInstance != null && that.ObjectInstance != null)
            {
                if (!ObjectInstance.Equals(that.ObjectInstance))
                    return false;
            }
            else
            {
                if (ObjectInstance != null || that.ObjectInstance != null)
                    return false;
            }
            return base.Equals(obj) && Name == that.Name;
        }
    }
    class FieldDeclarationNode : FieldNode
    {
        public IExpressionNode Initializer { get; set; }
        public FieldDeclarationNode(string name, TypeReference type) : base(name, type)
        {
        }
        public FieldDefinition CreateFieldDefinition(FieldAttributes attributes)
        {
            if (ReturnType != null)
                return Field = new FieldDefinition(Name, attributes, ReturnType);
            else
                throw new TypeException("Cannot create a field without a declared type");
        }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is FieldDeclarationNode))
                return false;
            var that = (FieldDeclarationNode)obj;
            if (Initializer != null && that.Initializer != null)
            {
                if (!Initializer.Equals(that.Initializer))
                    return false;
            }
            else
            {
                if (Initializer != null || that.Initializer != null)
                    return false;
            }
            return base.Equals(obj);
        }
    }

    /*class PropertyNode : LValueNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyDefinition Property { get; }
    }*/
}
