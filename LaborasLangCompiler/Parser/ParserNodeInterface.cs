using LaborasLangCompiler.Codegen;
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
        Literal,
        Function,
        Call,
        ObjectCreation,
        BinaryOperator,
        UnaryOperator,
        AssignmentOperator,
        This,
        LocalVariable,
        Field,
        Property,
        FunctionArgument,
        Type,
        ParserInternal
    }

    interface IExpressionNode : IParserNode
    {
        ExpressionNodeType ExpressionType { get; }
        TypeReference ExpressionReturnType { get; }
    }

    interface ILiteralNode : IExpressionNode
    {
        Literal Value { get; }
    }

    interface IMethodNode : IExpressionNode
    {
        IExpressionNode ObjectInstance { get; }
        MethodReference Method { get; }
    }

    interface IFunctionCallNode : IExpressionNode
    {
        IReadOnlyList<IExpressionNode> Args { get; }
        IExpressionNode Function { get; }
    }

    interface IObjectCreationNode : IExpressionNode
    {
        MethodReference Constructor { get; }
        IReadOnlyList<IExpressionNode> Args { get; }
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

    interface ILocalVariableNode : IExpressionNode
    {
        VariableDefinition LocalVariable { get; }
    }

    interface IFieldNode : IExpressionNode
    {
        IExpressionNode ObjectInstance { get; }
        FieldReference Field { get; }
    }

    interface IPropertyNode : IExpressionNode
    {
        IExpressionNode ObjectInstance { get; }
        PropertyReference Property { get; }
    }

    interface IParameterNode : IExpressionNode
    {
        ParameterDefinition Parameter { get; }
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

    interface IBinaryOperatorNode : IExpressionNode
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

    interface IUnaryOperatorNode : IExpressionNode
    {
        UnaryOperatorNodeType UnaryOperatorType { get; }
        IExpressionNode Operand { get; }
    }

    interface IAssignmentOperatorNode : IExpressionNode
    {
        IExpressionNode LeftOperand { get; }
        IExpressionNode RightOperand { get; }
    }

    interface ISymbolDeclarationNode : IParserNode
    {
        VariableDefinition Variable { get; }
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
