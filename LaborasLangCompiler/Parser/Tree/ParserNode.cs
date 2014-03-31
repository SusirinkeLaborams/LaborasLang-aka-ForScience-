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
            Operand,
            Action,
            Function
        }

        public abstract Type NodeType { get; }
        public abstract ParserNode Parent { get; }
    }

    // Literals, function calls, function arguments, local variables and fields
    abstract class OperandNode : ParserNode
    {
        public enum Kind
        {
            LValue,
            RValue
        }

        public abstract Kind OperandKind { get; }
        public abstract TypeReference OperandType { get; }
    }

    abstract class RValueOperandNode : OperandNode
    {
        public enum RValueKind
        {
            Literal,
            FunctionCall
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
        public abstract IReadOnlyList<OperandNode> Arguments { get; }
    }

    abstract class LValueNodeOperandNode : OperandNode
    {
        public enum LValueKind
        {
            LocalVariable,
            Field,
            FunctionArgument
        }

        public abstract LValueKind LValueOperandKind { get; }
    }

    abstract class LocalVariableNode : LValueNodeOperandNode
    {
        public abstract VariableDefinition LocalVariable { get; }
    }

    abstract class FieldNode : LValueNodeOperandNode
    {
        public abstract FieldDefinition Field { get; }
    }

    abstract class FunctionArgument : LValueNodeOperandNode
    {
        public abstract string Name { get; }
    }

    abstract class ActionNode : ParserNode
    {
        public enum ActionType
        {
            BinaryOperator,
            UnaryOperator,
            AssignmentOperator,
            SymbolDeclaration
        }
    }

    abstract class BinaryOperatorNode : ActionNode
    {
        public enum Kind
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

        public abstract Kind OperatorKind { get; }
        public abstract OperandNode LeftOperand { get; }
        public abstract OperandNode RightOperand { get; }
    }

    abstract class UnaryOperatorNode : ActionNode
    {
        public enum Kind
        {
            Negation,            
            PreIncrement,
            PreDecrement,
            PostIncrement,
            PostDecrement,
            VoidOperator    // Discards Operand result
        }

        public abstract Kind OperatorKind { get; }
        public abstract OperandNode Operand { get; }
    }

    abstract class AssignmentOperatorNode : ActionNode
    {
        public abstract LValueNodeOperandNode LeftOperand { get; }
        public abstract OperandNode RightOperand { get; }
    }

    abstract class SymbolDeclaration : ActionNode
    {
        public abstract LocalVariableNode LocalVariable { get; }
        public abstract OperandNode Initializer { get; }
    }

    abstract class Function : ParserNode
    {
        public abstract MethodEmitter Method { get; }
        public abstract List<ParserNode> Body { get; }
    }
}
