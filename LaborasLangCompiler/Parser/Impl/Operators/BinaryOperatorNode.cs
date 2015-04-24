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
using System.Diagnostics.Contracts;
using Lexer;

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

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.InfixNode);
            var left = ExpressionNode.Parse(context, lexerNode.Children[0]);
            var right = ExpressionNode.Parse(context, lexerNode.Children[1]);
            var opType = lexerNode.Children[2].Type;

            return Create(context, Operators[opType], left, right, context.Parser.GetSequencePoint(lexerNode));
        }

        public static ExpressionNode Create(ContextNode context, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            if (!left.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(left.SequencePoint, "Binary operand is not gettable");

            if (!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Binary operand is not gettable");

            ExpressionNode ret = AsOverload(context, op, left, right, point);
            if (ret == null)
                ret = AsBuiltIn(context, op, left, right, point);

            if (ret == null)
                OperatorMissmatch(point, op, left.ExpressionReturnType, right.ExpressionReturnType);

            Contract.Assume(ret != null);
            return ret;
        }

        private static ExpressionNode AsBuiltIn(ContextNode context, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            var instance = new BinaryOperatorNode(point);
            instance.left = left;
            instance.right = right;
            instance.BinaryOperatorType = op;
            bool verified = false;
            switch (op)
            {
                case BinaryOperatorNodeType.Addition:
                case BinaryOperatorNodeType.Subtraction:
                case BinaryOperatorNodeType.Multiplication:
                case BinaryOperatorNodeType.Division:
                case BinaryOperatorNodeType.Modulus:
                    verified = instance.VerifyArithmetic(context.Parser);
                    break;
                case BinaryOperatorNodeType.GreaterThan:
                case BinaryOperatorNodeType.LessThan:
                case BinaryOperatorNodeType.GreaterEqualThan:
                case BinaryOperatorNodeType.LessEqualThan:
                case BinaryOperatorNodeType.Equals:
                case BinaryOperatorNodeType.NotEquals:
                    verified = instance.VerifyComparison(context.Parser);
                    break;
                case BinaryOperatorNodeType.ShiftLeft:
                case BinaryOperatorNodeType.ShiftRight:
                    verified = instance.VerifyShift(context.Parser);
                    break;
                case BinaryOperatorNodeType.LogicalAnd:
                case BinaryOperatorNodeType.LogicalOr:
                    verified = instance.VerifyLogical(context.Parser);
                    break;
                case BinaryOperatorNodeType.BinaryAnd:
                case BinaryOperatorNodeType.BinaryOr:
                case BinaryOperatorNodeType.BinaryXor:
                    verified = instance.VerifyBinary();
                    break;
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(point, "Binary op expected, '{0} found", op);
                    return null;//unreachable
            }
            return verified ? instance : null;
        }

        private static ExpressionNode AsOverload(ContextNode context, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            var name = Overloads[op];
            return AsOverload(context, name, left, right, point);
        }

        public static ExpressionNode AsOverload(ContextNode context, string name, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            var methods = TypeUtils.GetOperatorMethods(context.Assembly, left, right, name);

            var args = Utils.Utils.Enumerate(left, right);
            var argsTypes = args.Select(a => a.ExpressionReturnType).ToList();

            methods = methods.Where(m => MetadataHelpers.MatchesArgumentList(m, argsTypes));

            var method = AssemblyRegistry.GetCompatibleMethod(methods, argsTypes);

            if (method != null)
            {
                return MethodCallNode.Create(context, new MethodNode(method, null, context, point), args, point);
            }
            else
            {
                if (methods.Count() == 0)
                {
                    return null;
                }
                else
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point,
                        "Overloaded operator {0} for operands {1} and {2} is ambiguous",
                        name, left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName);
                    return null;//unreachable
                }
            }
        }

        private bool VerifyArithmetic(Parser parser)
        {
            if (left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType())
            {
                bool comparable = false;
                if (left.IsAssignableTo(right))
                {
                    type = right.ExpressionReturnType;
                    comparable = true;
                }
                else if (right.IsAssignableTo(left))
                {
                    type = left.ExpressionReturnType;
                    comparable = true;
                }

                if (comparable && BinaryOperatorType == BinaryOperatorNodeType.Modulus)
                    comparable = left.ExpressionReturnType.IsIntegerType() && right.ExpressionReturnType.IsIntegerType();

                return comparable;
            }

            if (BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                type = parser.String;
                return left.ExpressionReturnType.IsStringType() || right.ExpressionReturnType.IsStringType();
            }

            return false;
        }

        private bool VerifyComparison(Parser parser)
        {
            type = parser.Bool;

            bool assignable = left.IsAssignableTo(right) || right.IsAssignableTo(left);

            if (BinaryOperatorType == BinaryOperatorNodeType.Equals || BinaryOperatorType == BinaryOperatorNodeType.NotEquals)
                return assignable;

            return assignable && parser.IsPrimitive(left.ExpressionReturnType) && parser.IsPrimitive(right.ExpressionReturnType);
        }

        private bool VerifyShift(Parser parser)
        {
            //probably wrong
            type = left.ExpressionReturnType;

            if (right.ExpressionReturnType.FullName != parser.Int32.FullName)
                return false;

            if (!left.ExpressionReturnType.IsIntegerType())
                return false;

            return true;
        }

        private bool VerifyBinary()
        {
            type = left.ExpressionReturnType;

            if (!(left.ExpressionReturnType.IsIntegerType() && right.ExpressionReturnType.IsIntegerType()))
                return false;

            if (left.ExpressionReturnType.GetPrimitiveWidth() != right.ExpressionReturnType.GetPrimitiveWidth())
                return false;

            return true;
        }

        private bool VerifyLogical(Parser parser)
        {
            type = parser.Bool;

            return left.ExpressionReturnType.IsBooleanType() && right.ExpressionReturnType.IsBooleanType();
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

        private static void OperatorMissmatch(SequencePoint point, BinaryOperatorNodeType op, TypeReference left, TypeReference right)
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(point,
                "Unable to perform {0} on operands {1} and {2}, no built int operation or operaror overload found",
                op, left.FullName, right.FullName);
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
            {Lexer.TokenType.RightShift, BinaryOperatorNodeType.ShiftRight},
            {Lexer.TokenType.LeftShift, BinaryOperatorNodeType.ShiftLeft}
        };

        private static Dictionary<BinaryOperatorNodeType, string> Overloads = new Dictionary<BinaryOperatorNodeType, string>()
        {
            {BinaryOperatorNodeType.Addition, "op_Addition"}, 
            {BinaryOperatorNodeType.Subtraction, "op_Subtraction"}, 
            {BinaryOperatorNodeType.Multiplication, "op_Multiply"}, 
            {BinaryOperatorNodeType.Division, "op_Division"}, 
            {BinaryOperatorNodeType.Modulus, "op_Modulus"}, 
            {BinaryOperatorNodeType.BinaryOr, "op_BitwiseOr"}, 
            {BinaryOperatorNodeType.BinaryAnd, "op_BitwiseAnd"}, 
            {BinaryOperatorNodeType.BinaryXor, "op_ExclusiveOr"}, 
            {BinaryOperatorNodeType.GreaterThan, "op_GreaterThan"}, 
            {BinaryOperatorNodeType.GreaterEqualThan, "op_GreaterThanOrEqual"}, 
            {BinaryOperatorNodeType.LessThan, "op_LessThan"}, 
            {BinaryOperatorNodeType.LessEqualThan, "op_LessThanOrEqual"}, 
            {BinaryOperatorNodeType.Equals, "op_Equality"}, 
            {BinaryOperatorNodeType.NotEquals, "op_Inequality"}, 
            {BinaryOperatorNodeType.LogicalOr, "op_LogicalOr"}, 
            {BinaryOperatorNodeType.LogicalAnd, "op_LogicalAnd"}, 
            {BinaryOperatorNodeType.ShiftRight, "op_RightShift"},
            {BinaryOperatorNodeType.ShiftLeft, "op_LeftShift"}
        };
    }
}
