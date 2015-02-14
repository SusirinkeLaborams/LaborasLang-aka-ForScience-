using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using LaborasLangCompiler.Common;

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

        public static ExpressionNode Parse(Parser parser, ContextNode parent, AstNode lexerNode)
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
                    left = Create(parser, Operators[lexerNode.Children[i].Type], left, right);
                }
                return left;
            }
        }

        public static BinaryOperatorNode Create(Parser parser, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right)
        {
            var instance = new BinaryOperatorNode(left.SequencePoint);
            instance.BinaryOperatorType = op;
            instance.left = left;
            instance.right = right;

            if (!left.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(left.SequencePoint, "Binary operand is not gettable");

            if (!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Binary operand is not gettable");

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
                    ErrorCode.InvalidStructure.ReportAndThrow(instance.SequencePoint, "Binary op expected, '{0} found", op);
                    break;//unreachable
            }
            return instance;
        }

        private void VerifyArithmetic(Parser parser)
        {
            if (left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType())
            {
                if (left.IsAssignableTo(right))
                {
                    type = right.ExpressionReturnType;
                }
                else if (right.IsAssignableTo(left))
                {
                    type = left.ExpressionReturnType;
                }
                else
                {
                    ArithmeticMissmatch();
                }
            }
            else if ((left.ExpressionReturnType.IsStringType() || right.ExpressionReturnType.IsStringType()) && BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                type = parser.String;
            }
            else
            {
                ArithmeticMissmatch();
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
                ComparisonMissmatch();
        }

        private void VerifyShift(Parser parser)
        {
            type = left.ExpressionReturnType;
            if (right.ExpressionReturnType.FullName != parser.Int32.FullName)
                ShiftMissmatch();
            if (!left.ExpressionReturnType.IsIntegerType())
                ShiftMissmatch();
        }

        private void VerifyBinary()
        {
            type = left.ExpressionReturnType;

            if (!(left.ExpressionReturnType.IsIntegerType() && right.ExpressionReturnType.IsIntegerType()))
                BinaryMissmatch();

            if (left.ExpressionReturnType.GetIntegerWidth() != right.ExpressionReturnType.GetIntegerWidth())
                BinaryMissmatch();
        }

        private void VerifyLogical(Parser parser)
        {
            type = parser.Bool;

            if (!(left.ExpressionReturnType.IsBooleanType() && right.ExpressionReturnType.IsBooleanType()))
                LogicalMissmatch();
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

        private void ArithmeticMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Cannot perform arithmetic operation '{0}' on types {1} and {2}", 
                BinaryOperatorType, LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
        }

        private void ComparisonMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Cannot perform comparison on types {0} and {1}",
                LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
        }

        private void LogicalMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Cannot perform logical operations on types {0} and {1}, boolean required",
                LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
        }

        private void BinaryMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Cannot perform binary operations on types {0} and {1}, integers of equal length required",
                LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
        }

        private void ShiftMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Cannot perform shift operations on types {0} and {1}, left must be an integer, right must be an integer up to 32 bytes long",
                LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
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
