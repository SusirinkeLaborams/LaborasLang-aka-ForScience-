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

        public static new LValueNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            if(lexerNode.Token.Name == "Symbol")
            {
                var value = parser.GetNodeValue(lexerNode);
                var instance = parent.GetSymbol(value);
                if(instance == null)
                    parser.Root.GetSymbol(value);
                if (instance != null)
                    return instance;
                else
                    throw new SymbolNotFoundException("Symbol " + value + " not found");
            }
            throw new NotImplementedException();
        }
    }

    class LocalVariableNode : LValueNode, ILocalVariableNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; private set; }
        public override TypeReference ReturnType { get; protected set; }
        public LocalVariableNode(TypeReference type, VariableDefinition variable)
        {
            ReturnType = type;
            LocalVariable = variable;
        }
    }

    /*class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; }
        public FieldDefinition Field { get; }
    }

    class PropertyNode : LValueNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyDefinition Property { get; }
    }

    class FunctionArgumentNode : LValueNode, IFunctionArgumentNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; }
    }*/
}
