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

        public static ExpressionNode Parse(ContextNode context, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(context, lexerNode.Children[0]);
            }
            else
            {
                ExpressionNode left, right;
                left = ExpressionNode.Parse(context, lexerNode.Children[0]);
                
                for (int i = 1; i < lexerNode.Children.Count; i += 2)
                {
                    right = ExpressionNode.Parse(context, lexerNode.Children[i + 1]);
                    left = Create(context, Operators[lexerNode.Children[i].Type], left, right);
                }
                return left;
            }
        }

        public static ExpressionNode Create(ContextNode context, BinaryOperatorNodeType op, ExpressionNode left, ExpressionNode right)
        {
            var instance = new BinaryOperatorNode(left.SequencePoint);
            instance.BinaryOperatorType = op;
            instance.left = left;
            instance.right = right;

            if (!left.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(left.SequencePoint, "Binary operand is not gettable");

            if (!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Binary operand is not gettable");

            if (!instance.VerifyBuiltIn(context.Parser))
            {
                return instance.AsOverload(context);
            }
            
            return instance;
        }

        private bool VerifyBuiltIn(Parser parser)
        {
            switch (BinaryOperatorType)
            {
                case BinaryOperatorNodeType.Addition:
                case BinaryOperatorNodeType.Subtraction:
                case BinaryOperatorNodeType.Multiplication:
                case BinaryOperatorNodeType.Division:
                case BinaryOperatorNodeType.Modulus:
                    return VerifyArithmetic(parser);
                case BinaryOperatorNodeType.GreaterThan:
                case BinaryOperatorNodeType.LessThan:
                case BinaryOperatorNodeType.GreaterEqualThan:
                case BinaryOperatorNodeType.LessEqualThan:
                case BinaryOperatorNodeType.Equals:
                case BinaryOperatorNodeType.NotEquals:
                    return VerifyComparison(parser);
                case BinaryOperatorNodeType.ShiftLeft:
                case BinaryOperatorNodeType.ShiftRight:
                    return VerifyShift(parser);
                case BinaryOperatorNodeType.LogicalAnd:
                case BinaryOperatorNodeType.LogicalOr:
                    return VerifyLogical(parser);
                case BinaryOperatorNodeType.BinaryAnd:
                case BinaryOperatorNodeType.BinaryOr:
                case BinaryOperatorNodeType.BinaryXor:
                    return VerifyBinary();
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(SequencePoint, "Binary op expected, '{0} found", BinaryOperatorType);
                    return false;//unreachable
            }
        }

        private ExpressionNode AsOverload(ContextNode context)
        {
            var name = Overloads[BinaryOperatorType];
            var methods = AssemblyRegistry.GetMethods(context.Parser.Assembly, left.ExpressionReturnType, name)
                .Union(AssemblyRegistry.GetMethods(context.Parser.Assembly, right.ExpressionReturnType, name));

            var args = Utils.Utils.Enumerate(left, right);
            var argsTypes = args.Select(a => a.ExpressionReturnType).ToList();

            methods = methods.Where(m => MetadataHelpers.MatchesArgumentList(m, argsTypes));

            var method = AssemblyRegistry.GetCompatibleMethod(methods, argsTypes);

            if(method != null)
            {
                return MethodCallNode.Create(context, new MethodNode(method, null, context, SequencePoint), args, SequencePoint);
            }
            else
            {
                if (methods.Count() == 0)
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                        "No operator ({0}) matches operands {1} and {2}",
                        BinaryOperatorType, left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName);
                    return null;//unreachable
                }
                else
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                        "Overloaded operator ({0}) for operands {1} and {2} is ambiguous",
                        BinaryOperatorType, left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName);
                    return null;//unreachable
                }
            }
        }

        private bool VerifyArithmetic(Parser parser)
        {
            if (left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType())
            {
                if (left.IsAssignableTo(right))
                {
                    type = right.ExpressionReturnType;
                    return true;
                }
                else if (right.IsAssignableTo(left))
                {
                    type = left.ExpressionReturnType;
                    return true;
                }
            }
            else if ((left.ExpressionReturnType.IsStringType() || right.ExpressionReturnType.IsStringType()) && BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                type = parser.String;
                return true;
            }

            return false;
        }

        private bool VerifyComparison(Parser parser)
        {
            type = parser.Bool;

            //probably wrong
            bool comparable = left.ExpressionReturnType.IsNumericType() && right.ExpressionReturnType.IsNumericType();

            //wtf, this is wrong, replace with overloads
            /*if (!comparable)
                comparable = left.ExpressionReturnType.IsStringType() && right.ExpressionReturnType.IsStringType();*/

            if (!comparable)
                comparable = left.ExpressionReturnType.IsBooleanType() && right.ExpressionReturnType.IsBooleanType();

            if (comparable)
                comparable = left.IsAssignableTo(right) || right.IsAssignableTo(left);

            return comparable;
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

            if (left.ExpressionReturnType.GetIntegerWidth() != right.ExpressionReturnType.GetIntegerWidth())
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

        private void OperatorMissmatch()
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint,
                "Unable to perform {0} on operands {1} and {2}, no built int operation or operaror overload found",
                BinaryOperatorType, LeftOperand.ExpressionReturnType.FullName, RightOperand.ExpressionReturnType.FullName);
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
