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
        public override TypeWrapper TypeWrapper { get { return typeWrapper; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private TypeWrapper typeWrapper;
        private ExpressionNode left, right;
        protected BinaryOperatorNode(SequencePoint point) : base(point) { }
        public static new ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            }
            else
            {
                ExpressionNode left, right;
                left = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                for (int i = 1; i < lexerNode.Children.Count; i += 2)
                {
                    right = ExpressionNode.Parse(parser, parent, lexerNode.Children[i + 1]);
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
                    instance.VerifyBinary(parser);
                    break;
                default:
                    throw new ParseException(instance.SequencePoint, "Binary op expected, '{0}' received", op);
            }
            return instance;
        }
        private void VerifyArithmetic(Parser parser)
        {
            if (left.TypeWrapper.IsNumericType() && right.TypeWrapper.IsNumericType())
            {
                if (left.TypeWrapper.IsAssignableTo(right.TypeWrapper))
                    typeWrapper = right.TypeWrapper;
                else if (right.TypeWrapper.IsAssignableTo(left.TypeWrapper))
                    typeWrapper = left.TypeWrapper;
                else
                    throw new TypeException(SequencePoint, "Incompatible operand types, {0} and {1} received",
                        left.TypeWrapper.FullName, right.TypeWrapper.FullName);
            }
            else if ((left.TypeWrapper.IsStringType() || right.TypeWrapper.IsStringType()) && BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                typeWrapper = parser.String;
            }
            else
            {
                throw new TypeException(SequencePoint, "Incompatible operand types, {0} and {1} for operator {2}",
                    left.TypeWrapper.FullName, right.TypeWrapper.FullName, BinaryOperatorType);
            }
        }
        private void VerifyComparison(Parser parser)
        {
            typeWrapper = parser.Bool;

            bool comparable = left.TypeWrapper.IsNumericType() && right.TypeWrapper.IsNumericType();

            if (!comparable)
                comparable = left.TypeWrapper.IsStringType() && right.TypeWrapper.IsStringType();

            if (!comparable)
                comparable = left.TypeWrapper.IsBooleanType() && right.TypeWrapper.IsBooleanType();

            if (comparable)
                comparable = left.TypeWrapper.IsAssignableTo(right.TypeWrapper) || right.TypeWrapper.IsAssignableTo(left.TypeWrapper);

            if (!comparable)
                throw new TypeException(SequencePoint, "Types {0} and {1} cannot be compared with op {2}",
                    left.TypeWrapper, right.TypeWrapper, BinaryOperatorType);
        }
        private void VerifyShift(Parser parser)
        {
            typeWrapper = left.TypeWrapper;
            if (right.TypeWrapper.FullName != parser.Int32.FullName)
                throw new TypeException(SequencePoint, "Right shift operand must be of signed 32bit integer type");
            if (!left.TypeWrapper.IsIntegerType())
                throw new TypeException(SequencePoint, "Left shift operand must be of integer type");
        }
        private void VerifyBinary(Parser parser)
        {
            typeWrapper = left.TypeWrapper;

            if (!(left.TypeWrapper.IsIntegerType() && right.TypeWrapper.IsIntegerType()))
                throw new TypeException(SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.TypeWrapper, right.TypeWrapper);

            if (left.TypeWrapper.GetIntegerWidth() != right.TypeWrapper.GetIntegerWidth())
                throw new TypeException(SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.TypeWrapper, right.TypeWrapper);
        }
        private void VerifyLogical(Parser parser)
        {
            typeWrapper = parser.Bool;

            if (!(left.TypeWrapper.IsBooleanType() && right.TypeWrapper.IsBooleanType()))
                throw new TypeException(SequencePoint, "Logical operations only allowed on booleans, operands: {0}, {1}",
                    left.TypeWrapper, right.TypeWrapper);
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

        public static Dictionary<Lexer.TokenType, BinaryOperatorNodeType> Operators;
        static BinaryOperatorNode()
        {
            Operators = new Dictionary<Lexer.TokenType, BinaryOperatorNodeType>();
            Operators[Lexer.TokenType.Plus]  = BinaryOperatorNodeType.Addition;
            Operators[Lexer.TokenType.Minus]  = BinaryOperatorNodeType.Subtraction;
            Operators[Lexer.TokenType.Multiply] = BinaryOperatorNodeType.Multiplication;
            Operators[Lexer.TokenType.Divide]  = BinaryOperatorNodeType.Division;
            Operators[Lexer.TokenType.Remainder]  = BinaryOperatorNodeType.Modulus;
            Operators[Lexer.TokenType.BitwiseOr]  = BinaryOperatorNodeType.BinaryOr;
            Operators[Lexer.TokenType.BitwiseAnd]  = BinaryOperatorNodeType.BinaryAnd;
            Operators[Lexer.TokenType.BitwiseXor]  = BinaryOperatorNodeType.BinaryXor;
            Operators[Lexer.TokenType.More]  = BinaryOperatorNodeType.GreaterThan;
            Operators[Lexer.TokenType.MoreOrEqual] = BinaryOperatorNodeType.GreaterEqualThan;
            Operators[Lexer.TokenType.Less]  = BinaryOperatorNodeType.LessThan;
            Operators[Lexer.TokenType.LessOrEqual] = BinaryOperatorNodeType.LessEqualThan;
            Operators[Lexer.TokenType.Equal] = BinaryOperatorNodeType.Equals;
            Operators[Lexer.TokenType.NotEqual] = BinaryOperatorNodeType.NotEquals;
            Operators[Lexer.TokenType.LogicalOr] = BinaryOperatorNodeType.LogicalOr;
            Operators[Lexer.TokenType.LogicalAnd] = BinaryOperatorNodeType.LogicalAnd;
            Operators[Lexer.TokenType.RightShift] = BinaryOperatorNodeType.ShiftRight;
            Operators[Lexer.TokenType.LeftShift] = BinaryOperatorNodeType.ShiftLeft;
        }
    }
}
