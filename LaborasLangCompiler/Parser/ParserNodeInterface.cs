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

    [ContractClass(typeof(ILiteralNodeContract))]
    interface ILiteralNode : IExpressionNode
    {
        Literal Value { get; }
    }

    [ContractClass(typeof(IMethodNodeContract))]
    interface IMethodNode : IExpressionNode
    {
        IExpressionNode ObjectInstance { get; }
        MethodReference Method { get; }
    }

    [ContractClass(typeof(IFunctionCallNodeContract))]
    interface IFunctionCallNode : IExpressionNode
    {
        IReadOnlyList<IExpressionNode> Args { get; }
        IExpressionNode Function { get; }
    }

    [ContractClass(typeof(IObjectCreationNodeContract))]
    interface IObjectCreationNode : IExpressionNode
    {
        MethodReference Constructor { get; }
        IReadOnlyList<IExpressionNode> Args { get; }
    }

    [ContractClass(typeof(IWhileBlockNodeContract))]
    interface IWhileBlockNode : IParserNode
    {
        IExpressionNode Condition { get; }
        ICodeBlockNode ExecutedBlock { get; }
    }

    [ContractClass(typeof(IConditionBlockContract))]
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


    [ContractClass(typeof(IFieldNodeContract))]
    interface IFieldNode : IExpressionNode
    {
        IExpressionNode ObjectInstance { get; }
        FieldReference Field { get; }
    }

    [ContractClass(typeof(IPropertyNodeContract))]
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

    [ContractClass(typeof(IUnaryOperatorNodeContract))]
    interface IUnaryOperatorNode : IExpressionNode
    {
        UnaryOperatorNodeType UnaryOperatorType { get; }
        IExpressionNode Operand { get; }
    }

    [ContractClass(typeof(IAssignmentOperatorNodeContract))]
    interface IAssignmentOperatorNode : IExpressionNode
    {
        IExpressionNode LeftOperand { get; }
        IExpressionNode RightOperand { get; }
    }

    [ContractClass(typeof(IArrayCreationNodeContract))]
    interface IArrayCreationNode : IExpressionNode
    {
        IReadOnlyList<IExpressionNode> Dimensions { get; }
        IReadOnlyList<IExpressionNode> Initializer { get; }
    }

    [ContractClass(typeof(ISymbolDeclarationNodeContract))]
    interface ISymbolDeclarationNode : IParserNode
    {
        VariableDefinition Variable { get; }
        IExpressionNode Initializer { get; }
    }

    [ContractClass(typeof(ICodeBlockNodeContract))]
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
        public abstract NodeType Type { get; }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract SequencePoint SequencePoint { get; }

        public TypeReference ExpressionReturnType
        {
            get 
            {
                Contract.Ensures(Contract.Result<TypeReference>() != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(ILiteralNode))]
    abstract class ILiteralNodeContract : ILiteralNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        
        public Literal Value
        {
            get 
            {
                Contract.Ensures(Contract.Result<Literal>().Value != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IMethodNode))]
    abstract class IMethodNodeContract : IMethodNode
    {
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public IExpressionNode ObjectInstance
        {
            get
            {
                var result = Contract.Result<IExpressionNode>();
                Contract.Ensures(result == null || result.ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException();
            }
        }

        public MethodReference Method
        {
            get 
            {
                Contract.Ensures(Contract.Result<MethodReference>() != null);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(IFunctionCallNode))]
    abstract class IFunctionCallNodeContract : IFunctionCallNode
    {
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract ExpressionNodeType ExpressionType { get; }

        public IReadOnlyList<IExpressionNode> Args
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IExpressionNode>>() != null);
                throw new NotImplementedException();
            }
        }

        public IExpressionNode Function
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IObjectCreationNode))]
    abstract class IObjectCreationNodeContract : IObjectCreationNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract TypeReference ExpressionReturnType { get; }

        public MethodReference Constructor
        {
            get 
            {
                Contract.Ensures(Contract.Result<MethodReference>() != null);
                throw new NotImplementedException(); 
            }
        }

        public IReadOnlyList<IExpressionNode> Args
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IExpressionNode>>() != null);
                throw new NotImplementedException();
            }
        }

    }

    [ContractClassFor(typeof(IWhileBlockNode))]
    abstract class IWhileBlockNodeContract : IWhileBlockNode
    {
        public abstract SequencePoint SequencePoint { get; }
        public abstract NodeType Type { get; }

        public IExpressionNode Condition
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                Contract.Ensures(Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException(); 
            }
        }

        public ICodeBlockNode ExecutedBlock
        {
            get
            {
                Contract.Ensures(Contract.Result<ICodeBlockNode>() != null);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(IConditionBlock))]
    abstract class IConditionBlockContract : IConditionBlock
    {
        public abstract ICodeBlockNode FalseBlock { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract NodeType Type { get; }

        public IExpressionNode Condition
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                Contract.Ensures(Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException();
            }
        }

        public ICodeBlockNode TrueBlock
        {
            get
            {
                Contract.Ensures(Contract.Result<ICodeBlockNode>() != null);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(ILocalVariableNode))]
    abstract class ILocalVariableNodeContract : ILocalVariableNode
    {
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract ExpressionNodeType ExpressionType { get; }

        public VariableDefinition LocalVariable
        {
            get
            {
                Contract.Ensures(Contract.Result<VariableDefinition>() != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IFieldNode))]
    abstract class IFieldNodeContract : IFieldNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public IExpressionNode ObjectInstance
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() == null || Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException();
            }
        }

        public FieldReference Field
        {
            get
            {
                Contract.Ensures(Contract.Result<FieldReference>() != null);
                Contract.Ensures(Contract.Result<FieldReference>().FieldType != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IPropertyNode))]
    abstract class IPropertyNodeContract : IPropertyNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public IExpressionNode ObjectInstance
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() == null || Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException();
            }
        }

        public PropertyReference Property
        {
            get
            {
                Contract.Ensures(Contract.Result<PropertyReference>() != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IParameterNode))]
    abstract class IParameterNodeContract : IParameterNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public ParameterDefinition Parameter
        {
            get
            {
                Contract.Ensures(Contract.Result<ParameterDefinition>() != null);
                throw new NotImplementedException();
            }
        }
    }

    [ContractClassFor(typeof(IBinaryOperatorNode))]
    abstract class IBinaryOperatorNodeContract : IBinaryOperatorNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract BinaryOperatorNodeType BinaryOperatorType { get; }

        public IExpressionNode LeftOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                Contract.Ensures(Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException(); 
            }
        }

        public IExpressionNode RightOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                Contract.Ensures(Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(IUnaryOperatorNode))]
    abstract class IUnaryOperatorNodeContract : IUnaryOperatorNode
    {
        public abstract UnaryOperatorNodeType UnaryOperatorType { get; }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public IExpressionNode Operand
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpressionNode>() != null);
                Contract.Ensures(Contract.Result<IExpressionNode>().ExpressionType != ExpressionNodeType.ParserInternal);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(IAssignmentOperatorNode))]
    abstract class IAssignmentOperatorNodeContract : IAssignmentOperatorNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

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
    }
        
    [ContractClassFor(typeof(IArrayCreationNode))]
    abstract class IArrayCreationNodeContract : IArrayCreationNode
    {
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }
        public abstract IReadOnlyList<IExpressionNode> Initializer { get; }

        public IReadOnlyList<IExpressionNode> Dimensions
        {
            get 
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IExpressionNode>>() != null);
                Contract.Ensures(Contract.Result<IReadOnlyList<IExpressionNode>>().Count > 0);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(ISymbolDeclarationNode))]
    abstract class ISymbolDeclarationNodeContract : ISymbolDeclarationNode
    {
        public abstract IExpressionNode Initializer { get; }
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public VariableDefinition Variable
        {
            get
            {
                Contract.Ensures(Contract.Result<VariableDefinition>() != null);
                throw new NotImplementedException(); 
            }
        }
    }

    [ContractClassFor(typeof(ICodeBlockNode))]
    abstract class ICodeBlockNodeContract : ICodeBlockNode
    {
        public abstract NodeType Type { get; }
        public abstract SequencePoint SequencePoint { get; }

        public IReadOnlyList<IParserNode> Nodes
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IParserNode>>() != null);
                throw new NotImplementedException(); 
            }
        }
    }

#endregion Interface contracts
}
