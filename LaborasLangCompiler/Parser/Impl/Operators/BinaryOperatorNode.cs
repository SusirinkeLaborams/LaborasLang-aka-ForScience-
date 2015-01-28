using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class BinaryOperatorNode : ExpressionNode, IBinaryOperatorNode
    {
        public IExpressionNode RightOperand { get { return right; } }
        public IExpressionNode LeftOperand { get { return left; } }
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.BinaryOperator; } }
        public BinaryOperatorNodeType BinaryOperatorType { get; private set; }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private TypeReference type;
        private ExpressionNode left, right;

        protected BinaryOperatorNode(SequencePoint point) : base(point) { }

        public static ExpressionNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            }
            else
            {
                ExpressionNode left, right;
                left = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                if (!left.IsGettable)
                    throw new TypeException(left.SequencePoint, "Binary operands must be gettable");
                for (int i = 1; i < lexerNode.Children.Count; i += 2)
                {
                    right = ExpressionNode.Parse(parser, parent, lexerNode.Children[i + 1]);
                    if (!right.IsGettable)
                        throw new TypeException(left.SequencePoint, "Binary operands must be gettable");
                    left = Parse(parser, Operators[lexerNode.Children[i].Type], left, right);
                }
                return left;
            }
        }
        public static BinaryOperatorNode Parse(Parser parser, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right)
        {
            var instance = new BinaryOperatorNode(left.SequencePoint);
            instance.BinaryOperatorType = op;
            instance.left = left;
            instance.right = right;
            switch (instance.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.Addition:
                case BinaryOperatorNodeType.Subtraction:
                case BinaryOperatorNodeType.Multiplication:
                case BinaryOperatorNodeType.Division:
                case BinaryOperatorNodeType.Modulus:
                    instance.VerifyArithmetic(parser);
                    break;
                case BinaryOperatorNodeType.GreaterThan:
                case BinaryOperatorNodeType.LessThan:
                case BinaryOperatorNodeType.GreaterEqualThan:
                case BinaryOperatorNodeType.LessEqualThan:
                case BinaryOperatorNodeType.Equals:
                case BinaryOperatorNodeType.NotEquals:
                    instance.VerifyComparison(parser);
                    break;
                case BinaryOperatorNodeType.ShiftLeft:
                case BinaryOperatorNodeType.ShiftRight:
                    instance.VerifyShift(parser);
                    break;
                case BinaryOperatorNodeType.LogicalAnd:
                case BinaryOperatorNodeType.LogicalOr:
                    instance.VerifyLogical(parser);
                    break;
                case BinaryOperatorNodeType.BinaryAnd:
                case BinaryOperatorNodeType.BinaryOr:
                case BinaryOperatorNodeType.BinaryXor:
                    instance.VerifyBinary();
                    break;
                default:
                    throw new ParseException(instance.SequencePoint, "Binary op expected, '{0}' received", op);
            }
            return instance;
        }
        private void VerifyArithmetic(Parser parser)
        {
            if (left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType())
            {
                if (left.IsAssignableTo(right))
                    type = right.ExpressionReturnType;
                else if (right.IsAssignableTo(left))
                    type = left.ExpressionReturnType;
                else
                    throw new TypeException(SequencePoint, "Incompatible operand types, {0} and {1} received",
                        left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName);
            }
            else if ((left.ExpressionReturnType.IsStringType() || right.ExpressionReturnType.IsStringType()) && BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                type = parser.String;
            }
            else
            {
                throw new TypeException(SequencePoint, "Incompatible operand types, {0} and {1} for operator {2}",
                    left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName, BinaryOperatorType);
            }
        }
        private void VerifyComparison(Parser parser)
        {
            type = parser.Bool;

            bool comparable = left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType();

            if (!comparable)
                comparable = left.ExpressionReturnType.IsStringType() && right.ExpressionReturnType.IsStringType();

            if (!comparable)
                comparable = left.ExpressionReturnType.IsBooleanType() && right.ExpressionReturnType.IsBooleanType();

            if (comparable)
                comparable = left.IsAssignableTo(right) || right.IsAssignableTo(left);

            if (!comparable)
                throw new TypeException(SequencePoint, "Types {0} and {1} cannot be compared with op {2}",
                    left.ExpressionReturnType, right.ExpressionReturnType, BinaryOperatorType);
        }
        private void VerifyShift(Parser parser)
        {
            type = left.ExpressionReturnType;
            if (right.ExpressionReturnType.FullName != parser.Int32.FullName)
                throw new TypeException(SequencePoint, "Right shift operand must be of signed 32bit integer type");
            if (!left.ExpressionReturnType.IsIntegerType())
                throw new TypeException(SequencePoint, "Left shift operand must be of integer type");
        }
        private void VerifyBinary()
        {
            type = left.ExpressionReturnType;

            if (!(left.ExpressionReturnType.IsIntegerType() && right.ExpressionReturnType.IsIntegerType()))
                throw new TypeException(SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.ExpressionReturnType, right.ExpressionReturnType);

            if (left.ExpressionReturnType.GetIntegerWidth() != right.ExpressionReturnType.GetIntegerWidth())
                throw new TypeException(SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.ExpressionReturnType, right.ExpressionReturnType);
        }
        private void VerifyLogical(Parser parser)
        {
            type = parser.Bool;

            if (!(left.ExpressionReturnType.IsBooleanType() && right.ExpressionReturnType.IsBooleanType()))
                throw new TypeException(SequencePoint, "Logical operations only allowed on booleans, operands: {0}, {1}",
                    left.ExpressionReturnType, right.ExpressionReturnType);
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("BinaryOperator:");
            builder.Indent(indent + 1).AppendLine("Left:");
            builder.AppendLine(left.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Operator:");
            builder.Indent(indent + 2).AppendLine(BinaryOperatorType.ToString());
            builder.Indent(indent + 1).AppendLine("Right:");
            builder.AppendLine(right.ToString(indent + 2));
            return builder.ToString();
        }

        public static Dictionary<Lexer.TokenType, BinaryOperatorNodeType> Operators = new Dictionary<Lexer.TokenType, BinaryOperatorNodeType>()
        {
            {Lexer.TokenType.Plus, BinaryOperatorNodeType.Addition}, 
            {Lexer.TokenType.Minus, BinaryOperatorNodeType.Subtraction}, 
            {Lexer.TokenType.Multiply, BinaryOperatorNodeType.Multiplication}, 
            {Lexer.TokenType.Divide, BinaryOperatorNodeType.Division}, 
            {Lexer.TokenType.Remainder, BinaryOperatorNodeType.Modulus}, 
            {Lexer.TokenType.BitwiseOr, BinaryOperatorNodeType.BinaryOr}, 
            {Lexer.TokenType.BitwiseAnd, BinaryOperatorNodeType.BinaryAnd}, 
            {Lexer.TokenType.BitwiseXor, BinaryOperatorNodeType.BinaryXor}, 
            {Lexer.TokenType.More, BinaryOperatorNodeType.GreaterThan}, 
            {Lexer.TokenType.MoreOrEqual, BinaryOperatorNodeType.GreaterEqualThan}, 
            {Lexer.TokenType.Less, BinaryOperatorNodeType.LessThan}, 
            {Lexer.TokenType.LessOrEqual, BinaryOperatorNodeType.LessEqualThan}, 
            {Lexer.TokenType.Equal, BinaryOperatorNodeType.Equals}, 
            {Lexer.TokenType.NotEqual, BinaryOperatorNodeType.NotEquals}, 
            {Lexer.TokenType.LogicalOr, BinaryOperatorNodeType.LogicalOr}, 
            {Lexer.TokenType.LogicalAnd, BinaryOperatorNodeType.LogicalAnd}, 
            {Lexer.TokenType.RightShift, BinaryOperatorNodeType.ShiftRight}
        };
    }
}
