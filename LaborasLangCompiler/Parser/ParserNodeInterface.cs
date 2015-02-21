using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        ValueCreation,
        ArrayCreation,
        ParserInternal
    }

    [ContractClass(typeof(IExpressionNodeContract))]
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

    [ContractClass(typeof(ILocalVariableNodeContract))]
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

    [ContractClass(typeof(IParameterNodeContract))]
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

    [ContractClass(typeof(IBinaryOperatorNodeContract))]
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

    interface IArrayCreationNode : IExpressionNode
    {
        IReadOnlyList<IExpressionNode> Dimensions { get; }
        IReadOnlyList<IExpressionNode> Initializer { get; }
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

#region Interface contracts

    [ContractClassFor(typeof(IExpressionNode))]
    abstract class IExpressionNodeContract : IExpressionNode
    {
        public ExpressionNodeType ExpressionType
        {
            get { throw new NotImplementedException(); }
        }

        public TypeReference ExpressionReturnType
        {
            get 
            {
                Contract.Ensures(Contract.Result<TypeReference>() != null);
                throw new NotImplementedException();
            }
        }

        public NodeType Type
        {
            get { throw new NotImplementedException(); }
        }

        public SequencePoint SequencePoint
        {
            get { throw new NotImplementedException(); }
        }
    }

    [ContractClassFor(typeof(IBinaryOperatorNode))]
    abstract class IBinaryOperatorNodeContract : IBinaryOperatorNode
    {
        public BinaryOperatorNodeType BinaryOperatorType
        {
            get { throw new NotImplementedException(); }
        }

        public IExpressionNode LeftOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                throw new NotImplementedException(); 
            }
        }

        public IExpressionNode RightOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                throw new NotImplementedException(); 
            }
        }

        public ExpressionNodeType ExpressionType
        {
            get { throw new NotImplementedException(); }
        }

        public TypeReference ExpressionReturnType
        {
            get { throw new NotImplementedException(); }
        }

        public NodeType Type
        {
            get { throw new NotImplementedException(); }
        }

        public SequencePoint SequencePoint
        {
            get { throw new NotImplementedException(); }
        }
    }

    [ContractClassFor(typeof(IParameterNode))]
    abstract class IParameterNodeContract : IParameterNode
    {
        public ParameterDefinition Parameter
        {
            get 
            {
                Contract.Ensures(Contract.Result<ParameterDefinition>() != null);
                throw new NotImplementedException();
            }
        }

        public ExpressionNodeType ExpressionType
        {
            get { throw new NotImplementedException(); }
        }

        public TypeReference ExpressionReturnType
        {
            get { throw new NotImplementedException(); }
        }

        public NodeType Type
        {
            get { throw new NotImplementedException(); }
        }

        public SequencePoint SequencePoint
        {
            get { throw new NotImplementedException(); }
        }
    }

    [ContractClassFor(typeof(ILocalVariableNode))]
    abstract class ILocalVariableNodeContract : ILocalVariableNode
    {
        public VariableDefinition LocalVariable
        {
            get
            {
                Contract.Ensures(Contract.Result<VariableDefinition>() != null);
                throw new NotImplementedException();
            }
        }

        public ExpressionNodeType ExpressionType
        {
            get { throw new NotImplementedException(); }
        }

        public TypeReference ExpressionReturnType
        {
            get { throw new NotImplementedException(); }
        }

        public NodeType Type
        {
            get { throw new NotImplementedException(); }
        }

        public SequencePoint SequencePoint
        {
            get { throw new NotImplementedException(); }
        }
    }

#endregion
}
