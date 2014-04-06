using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Tree
{
    public enum NodeType
    {
        Expression,
        SymbolDeclaration,
        CodeBlockNode,
        ConditionBlock,
        WhileBlock,
        ImportNode
    }
    interface IParserNode
    {
        NodeType Type { get; }
    }

    // Literals, function calls, function arguments, local variables and fields

    public enum ExpressionNodeType
    {
        LValue,
        RValue
    }

    interface IExpressionNode : IParserNode
    {
        ExpressionNodeType ExpressionType { get; }
        TypeReference ReturnType { get; }
    }
    public enum RValueNodeType
    {
        Literal,
        Function,
        FunctionCall,
        MethodCall,
        ObjectCreation,
        BinaryOperator,
        UnaryOperator,
        AssignmentOperator
    }

    interface IRValueNode : IExpressionNode
    {
        RValueNodeType RValueType { get; }
    }

    interface ILiteralNode : IRValueNode
    {
        dynamic Value { get; }
    }

    interface IFunctionNode : IRValueNode
    {
        MethodReference Function { get; }
    }

    public enum CallNodeType
    {
        FunctionCall,
        MethodCall
    }

    interface ICallNode : IRValueNode
    {
        IReadOnlyList<IExpressionNode> Arguments { get; }
        CallNodeType CallType { get; }
    }

    interface IFunctionCallNode : ICallNode
    {
        IExpressionNode Function { get; }
    }

    interface IMethodCallNode : ICallNode
    {
        IExpressionNode ObjectInstance { get; }
        MethodReference Function { get; }
    }

    interface IObjectCreationNode : IRValueNode
    {
        IReadOnlyList<IExpressionNode> Arguments { get; }
    }

    public enum LValueNodeType
    {
        LocalVariable,
        Field,
        FunctionArgument
    }

    interface ILValueNode : IExpressionNode
    {
        LValueNodeType LValueType { get; }
    }

    interface ILocalVariableNode : ILValueNode
    {
        VariableDefinition LocalVariable { get; }
    }

    interface IFieldNode : ILValueNode
    {
        FieldDefinition Field { get; }
    }

    interface IFunctionArgumentNode : ILValueNode
    {
        ParameterDefinition Param { get; }
    }
    public enum BinaryOperatorNodeType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Remainder,
        BinaryOr,
        BinaryAnd,
        Xor,
        GreaterThan,
        GreaterEqualThan,
        LessThan,
        LessEqualThan,
        Equals,
        NotEquals,
        LogicalOr,
        LogicalAnd
    }

    interface IBinaryOperatorNode : IRValueNode
    {
        BinaryOperatorNodeType BinaryOperatorType { get; }
        IExpressionNode LeftOperand { get; }
        IExpressionNode RightOperand { get; }
    }

    public enum UnaryOperatorNodeType
    {
        BinaryNot,
        LogicalNot,
        Negation,
        PreIncrement,
        PreDecrement,
        PostIncrement,
        PostDecrement,
        VoidOperator    // Discards Operand result
    }

    interface IUnaryOperatorNode : IRValueNode
    {
        UnaryOperatorNodeType UnaryOperatorType { get; }
        IExpressionNode Operand { get; }
    }

    interface IAssignmentOperatorNode : IRValueNode
    {
        ILValueNode LeftOperand { get; }
        IExpressionNode RightOperand { get; }
    }

    interface ISymbolDeclarationNode : IParserNode
    {
        ILValueNode DeclaredSymbol { get; }
        IExpressionNode Initializer { get; }
    }

    interface ICodeBlockNode : IParserNode
    {
        IReadOnlyList<IParserNode> Nodes { get; }
    }
}
