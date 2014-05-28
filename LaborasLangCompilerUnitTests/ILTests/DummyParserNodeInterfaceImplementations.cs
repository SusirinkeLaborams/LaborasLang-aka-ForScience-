using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests
{
    class LiteralNode : ILiteralNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.Literal; } }

        public TypeReference ReturnType { get; set; }
        public dynamic Value { get; set; }
    }

    class FunctionNode : IFunctionNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.Function; } }

        public TypeReference ReturnType { get; set; }
        public IExpressionNode ObjectInstance { get; set; }
        public MethodReference Function { get; set; }
    }

    class MethodCallNode : IMethodCallNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IExpressionNode> arguments;

        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.Call; } }

        public TypeReference ReturnType { get; set; }
        public IExpressionNode Function { get; set; }

        public IReadOnlyList<IExpressionNode> Arguments
        {
            get { return arguments; }
            set
            {
                if (value is List<IExpressionNode>)
                {
                    arguments = (List<IExpressionNode>)value;
                }
                else
                {
                    arguments = value.ToList();
                }
            }
        }
    }

    class ObjectCreationNode : IObjectCreationNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IExpressionNode> arguments;

        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.ObjectCreation; } }

        public TypeReference ReturnType { get; set; }

        public IReadOnlyList<IExpressionNode> Arguments
        {
            get { return arguments; }
            set
            {
                if (value is List<IExpressionNode>)
                {
                    arguments = (List<IExpressionNode>)value;
                }
                else
                {
                    arguments = value.ToList();
                }
            }
        }
    }

    class LocalVariableNode : ILocalVariableNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public LValueNodeType LValueType { get { return LValueNodeType.LocalVariable; } }

        public TypeReference ReturnType { get { return LocalVariable.VariableType; } }
        public VariableDefinition LocalVariable { get; set; }
    }

    class FieldNode : IFieldNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public LValueNodeType LValueType { get { return LValueNodeType.Field; } }

        public TypeReference ReturnType { get { return Field.FieldType; } }
        public IExpressionNode ObjectInstance { get; set; }
        public FieldReference Field { get; set; }
    }

    class PropertyNode : IPropertyNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public LValueNodeType LValueType { get { return LValueNodeType.Property; } }

        public TypeReference ReturnType { get { return Property.PropertyType; } }
        public IExpressionNode ObjectInstance { get; set; }
        public PropertyReference Property { get; set; }
    }

    class FunctionArgumentNode : IFunctionArgumentNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }

        public TypeReference ReturnType { get { return Param.ParameterType; } }
        public ParameterDefinition Param { get; set; }
        public bool IsFunctionStatic { get; set; }
    }

    class BinaryOperatorNode : IBinaryOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.BinaryOperator; } }

        public TypeReference ReturnType { get; set; }
        public BinaryOperatorNodeType BinaryOperatorType { get; set; }
        public IExpressionNode LeftOperand { get; set; }
        public IExpressionNode RightOperand { get; set; }
    }

    class UnaryOperatorNode : IUnaryOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.UnaryOperator; } }

        public TypeReference ReturnType { get; set; }
        public UnaryOperatorNodeType UnaryOperatorType { get; set; }
        public IExpressionNode Operand { get; set; }
    }

    class AssignmentOperatorNode : IAssignmentOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public RValueNodeType RValueType { get { return RValueNodeType.AssignmentOperator; } }

        public TypeReference ReturnType { get { return LeftOperand.ReturnType; } }
        public ILValueNode LeftOperand { get; set; }
        public IExpressionNode RightOperand { get; set; }
    }

    class SymbolDeclarationNode : ISymbolDeclarationNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.SymbolDeclaration; } }

        public TypeReference ReturnType { get { return DeclaredSymbol.ReturnType; } }
        public ILValueNode DeclaredSymbol { get; set; }
        public IExpressionNode Initializer { get; set; }
    }

    class CodeBlockNode : ICodeBlockNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IParserNode> nodes;

        public NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes
        {
            get { return nodes; }
            set
            {
                if (value is List<IParserNode>)
                {
                    nodes = (List<IParserNode>)value;
                }
                else
                {
                    nodes = value.ToList();
                }
            }
        }
    }

    class ConditionBlockNode : IConditionBlock
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ConditionBlock; } }

        public IExpressionNode Condition { get; set; }
        public ICodeBlockNode TrueBlock { get; set; }
        public ICodeBlockNode FalseBlock { get; set; }
    }

    class WhileBlockNode : IWhileBlockNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.WhileBlock; } }

        public IExpressionNode Condition { get; set; }
        public ICodeBlockNode ExecutedBlock { get; set; }
    }

    class ReturnNode : IReturnNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ReturnNode; } }

        public IExpressionNode Expression { get; set; }
    }
}
