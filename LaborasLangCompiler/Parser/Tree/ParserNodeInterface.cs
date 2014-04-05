using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Tree
{
    abstract class ParserNode
    {        
        public enum Type
        {
            Expression,
            SymbolDeclaration,
            CodeBlockNode
        }

        public abstract Type NodeType { get; }
        public abstract ParserNode Parent { get; }
    }

    // Literals, function calls, function arguments, local variables and fields
    abstract class ExpressionNode : ParserNode
    {
        public enum Kind
        {
            LValue,
            RValue
        }

        public abstract Kind ExpressionKind { get; }
        public abstract TypeReference ExpressionType { get; }
    }

    abstract class RValueOperandNode : ExpressionNode
    {
        public enum RValueKind
        {
            Literal,
            FunctionCall,
            MethodCall,
            ObjectCreation,
            BinaryOperator,
            UnaryOperator,
            AssignmentOperator
        }

        public abstract RValueKind RValueOperandKind { get; }
    }

    abstract class LiteralNode : RValueOperandNode
    {
        public abstract dynamic Value { get; }
    }

    abstract class FunctionCallNode : RValueOperandNode
    {
        public abstract MethodReference Function { get; }
        public abstract IReadOnlyList<ExpressionNode> Arguments { get; }
    }

    abstract class MethodCallNode : FunctionCallNode
    {
        public abstract ExpressionNode ObjectInstance { get; }
    }

    abstract class ObjectCreationNode : RValueOperandNode
    {
        public abstract IReadOnlyList<ExpressionNode> Arguments { get; }
    }

    abstract class LValueOperandNode : ExpressionNode
    {
        public enum LValueKind
        {
            LocalVariable,
            Field,
            FunctionArgument
        }

        public abstract LValueKind LValueOperandKind { get; }
    }

    abstract class LocalVariableNode : LValueOperandNode
    {
        public abstract VariableDefinition LocalVariable { get; }
    }

    abstract class FieldNode : LValueOperandNode
    {
        public abstract FieldDefinition Field { get; }
    }

    abstract class FunctionArgumentNode : LValueOperandNode
    {
        public abstract string Name { get; }
    }

    abstract class BinaryOperatorNode : RValueOperandNode
    {
        public enum BinaryOperatorKind
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
            Less,
            Equals,
            NotEquals,
            LogicalOr,
            LogicalAnd
        }

        public abstract BinaryOperatorKind OperatorKind { get; }
        public abstract ExpressionNode LeftOperand { get; }
        public abstract ExpressionNode RightOperand { get; }
    }

    abstract class UnaryOperatorNode : RValueOperandNode
    {
        public enum UnaryOperatorKind
        {
            Negation,            
            PreIncrement,
            PreDecrement,
            PostIncrement,
            PostDecrement,
            VoidOperator    // Discards Operand result
        }

        public abstract UnaryOperatorKind OperatorKind { get; }
        public abstract ExpressionNode Operand { get; }
    }

    abstract class AssignmentOperatorNode : RValueOperandNode
    {
        public abstract LValueOperandNode LeftOperand { get; }
        public abstract ExpressionNode RightOperand { get; }
    }

    abstract class SymbolDeclarationNode : ParserNode
    {
        public abstract LocalVariableNode LocalVariable { get; }
        public abstract ExpressionNode Initializer { get; }
    }

    abstract class CodeBlockNode : ParserNode
    {
        public abstract IReadOnlyList<ParserNode> Nodes { get; }
    }
}
