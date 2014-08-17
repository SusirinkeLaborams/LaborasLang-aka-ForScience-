﻿using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    public enum NodeType
    {
        Expression,
        SymbolDeclaration,
        CodeBlockNode,
        ConditionBlock,
        WhileBlock,
        ReturnNode,
        ParserInternal
    }
    interface IParserNode
    {
        NodeType Type { get; }
        SequencePoint SequencePoint { get; }
    }

    // Literals, function calls, function arguments, local variables and fields

    public enum ExpressionNodeType
    {
        LValue,
        RValue,
        ParserInternal//used by the parser with incompletely parsed node
    }

    interface IExpressionNode : IParserNode
    {
        ExpressionNodeType ExpressionType { get; }
        TypeReference ExpressionReturnType { get; }
    }
    public enum RValueNodeType
    {
        Literal,
        Function,
        Call,
        ObjectCreation,
        BinaryOperator,
        UnaryOperator,
        AssignmentOperator,
        This,
        ParserInternal
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
        IExpressionNode ObjectInstance { get; }
        MethodReference Function { get; }
    }

    interface IMethodCallNode : IRValueNode
    {
        IReadOnlyList<IExpressionNode> Arguments { get; }
        IExpressionNode Function { get; }
    }

    interface IObjectCreationNode : IRValueNode
    {
        IReadOnlyList<IExpressionNode> Arguments { get; }
    }

    interface IWhileBlockNode : IParserNode
    {
        IExpressionNode Condition { get; }
        ICodeBlockNode ExecutedBlock { get; }
    }

    interface IConditionBlock : IParserNode
    {
        IExpressionNode Condition { get; }
        ICodeBlockNode TrueBlock { get; }
        ICodeBlockNode FalseBlock { get; }
    }

    public enum LValueNodeType
    {
        LocalVariable,
        Field,
        Property,
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
        IExpressionNode ObjectInstance { get; }
        FieldReference Field { get; }
    }

    interface IPropertyNode : ILValueNode
    {
        IExpressionNode ObjectInstance { get; }
        PropertyReference Property { get; }
    }

    interface IFunctionArgumentNode : ILValueNode
    {
        ParameterDefinition Param { get; }
        bool IsFunctionStatic { get; }
    }

    public enum BinaryOperatorNodeType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus,
        BinaryOr,
        BinaryAnd,
        BinaryXor,
        GreaterThan,
        GreaterEqualThan,
        LessThan,
        LessEqualThan,
        Equals,
        NotEquals,
        LogicalOr,
        LogicalAnd,
        ShiftRight,
        ShiftLeft
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
    interface IReturnNode : IParserNode
    {
        IExpressionNode Expression { get; }
    }
}
