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
            BinaryOperator,
            UnaryOperator,
            AssignmentOperator,
            SymbolDeclaration,
            Function
        }

        public Type Type { get; }
        private ParserNode parent { get; }
    }

    // Literals, function calls, function arguments, local variables and fields
    abstract class OperandNode : ParserNode
    {
        public enum Kind
        {
            LValue,
            RValue
        }

        public Kind Kind { get; }
        public TypeReference Type { get; }
    }

    abstract class RValueOperandNode : OperandNode
    {
        public enum RValueKind
        {
            Literal,
            FunctionCall
        }

        public RValueKind RValueKind { get; }
    }

    abstract class LiteralNode : RValueOperandNode
    {
        public dynamic Value { get; }
    }

    abstract class FunctionCallNode : RValueOperandNode
    {
        public MethodReference Function { get; }
        public IReadOnlyList<OperandNode> Arguments { get; }
    }

    abstract class LValueNodeOperandNode : OperandNode
    {
        public enum LValueKind
        {
            LocalVariable,
            Field,
            FunctionArgument
        }

        public LValueKind LValueKind { get; }
    }

    abstract class LocalVariableNode : LValueNodeOperandNode
    {
        public VariableDefinition LocalVariable { get; }
    }

    abstract class FieldNode : LValueNodeOperandNode
    {
        public FieldDefinition Field { get; }
    }

    abstract class FunctionArgument : LValueNodeOperandNode
    {
        public string Name { get; }
    }

    abstract class BinaryOperatorNode : ParserNode
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

        Kind Kind { get; }
        OperandNode LeftOperand { get; }
        OperandNode RightOperand { get; }
    }

    abstract class UnaryOperatorNode : ParserNode
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

        public Kind Kind { get; }
        public OperandNode Operand { get; }
    }

    abstract class AssignmentOperatorNode : ParserNode
    {
        public LValueNodeOperandNode LeftOperand { get; }
        public OperandNode RightOperand { get; }
    }

    abstract class SymbolDeclaration : ParserNode
    {
        public LocalVariableNode LocalVariable { get; }
        public OperandNode Initializer { get; }
    }

    abstract class Function : ParserNode
    {
        public MethodEmitter Method { get; }
        public List<ParserNode> Body { get; }
    }
}
