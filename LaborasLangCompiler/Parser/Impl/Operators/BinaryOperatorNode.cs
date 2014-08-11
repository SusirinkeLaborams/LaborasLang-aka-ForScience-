using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    class BinaryOperatorNode : RValueNode, IBinaryOperatorNode
    {
        public IExpressionNode RightOperand { get; private set; }
        public IExpressionNode LeftOperand { get; private set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.BinaryOperator; } }
        public BinaryOperatorNodeType BinaryOperatorType { get; set; }
        public override TypeWrapper ReturnType { get { return returnType; } }

        private TypeWrapper returnType;
        protected BinaryOperatorNode(SequencePoint point) : base(point) { }
        public static new ExpressionNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            }
            else
            {
                ExpressionNode left, right;
                string op;
                left = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                for (int i = 1; i < lexerNode.Children.Count; i += 2)
                {
                    op = parser.ValueOf(lexerNode.Children[i]);
                    right = ExpressionNode.Parse(parser, parent, lexerNode.Children[i + 1]);
                    left = Parse(parser, op, left, right);
                }
                return left;
            }
        }
        public static BinaryOperatorNode Parse(Parser parser, string op, ExpressionNode left, ExpressionNode right)
        {
            var instance = new BinaryOperatorNode(left.SequencePoint);
            instance.BinaryOperatorType = Operators[op];
            instance.LeftOperand = left;
            instance.RightOperand = right;
            switch (instance.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.Addition:
                case BinaryOperatorNodeType.Subtraction:
                case BinaryOperatorNodeType.Multiplication:
                case BinaryOperatorNodeType.Division:
                case BinaryOperatorNodeType.Modulus:
                    ParseArithmetic(parser, instance);
                    break;
                case BinaryOperatorNodeType.GreaterThan:
                case BinaryOperatorNodeType.LessThan:
                case BinaryOperatorNodeType.GreaterEqualThan:
                case BinaryOperatorNodeType.LessEqualThan:
                case BinaryOperatorNodeType.Equals:
                case BinaryOperatorNodeType.NotEquals:
                    ParseComparison(parser, instance);
                    break;
                case BinaryOperatorNodeType.ShiftLeft:
                case BinaryOperatorNodeType.ShiftRight:
                    ParseShift(parser, instance);
                    break;
                case BinaryOperatorNodeType.LogicalAnd:
                case BinaryOperatorNodeType.LogicalOr:
                    ParseLogical(parser, instance);
                    break;
                case BinaryOperatorNodeType.BinaryAnd:
                case BinaryOperatorNodeType.BinaryOr:
                case BinaryOperatorNodeType.BinaryXor:
                    ParseBinary(parser, instance);
                    break;
                default:
                    throw new ParseException(instance.SequencePoint, "Binary op expected, '{0}' received", op);
            }
            return instance;
        }
        private static void ParseArithmetic(Parser parser, BinaryOperatorNode instance)
        {
            var left = instance.LeftOperand;
            var right = instance.RightOperand;
            if (left.ReturnType.IsNumericType() && right.ReturnType.IsNumericType())
            {
                if (left.ReturnType.IsAssignableTo(right.ReturnType))
                    instance.returnType = right.ReturnType;
                else if (right.ReturnType.IsAssignableTo(left.ReturnType))
                    instance.returnType = left.ReturnType;
                else
                    throw new TypeException(instance.SequencePoint, "Incompatible operand types, {0} and {1} received",
                        left.ReturnType.FullName, right.ReturnType.FullName);
            }
            else if ((left.ReturnType.IsStringType() || right.ReturnType.IsStringType()) && instance.BinaryOperatorType == BinaryOperatorNodeType.Addition)
            {
                instance.returnType = parser.Primitives[Parser.String];
            }
            else
            {
                throw new TypeException(instance.SequencePoint, "Incompatible operand types, {0} and {1} for operator {2}", 
                    left.ReturnType.FullName, right.ReturnType.FullName, instance.BinaryOperatorType);
            }
        }
        private static void ParseComparison(Parser parser, BinaryOperatorNode instance)
        {
            var left = instance.LeftOperand;
            var right = instance.RightOperand;
            instance.returnType = parser.Primitives[Parser.Bool];

            bool comparable = left.ReturnType.IsNumericType() && right.ReturnType.IsNumericType();

            if (!comparable)
                comparable = left.ReturnType.IsStringType() && right.ReturnType.IsStringType();

            if (!comparable)
                comparable = left.ReturnType.IsBooleanType() && right.ReturnType.IsBooleanType();

            if (comparable)
                comparable = left.ReturnType.IsAssignableTo(right.ReturnType) || right.ReturnType.IsAssignableTo(left.ReturnType);

            if (!comparable)
                throw new TypeException(instance.SequencePoint, "Types {0} and {1} cannot be compared with op {2}", 
                    left.ReturnType, right.ReturnType, instance.BinaryOperatorType);
        }
        private static void ParseShift(Parser parser, BinaryOperatorNode instance)
        {
            var left = instance.LeftOperand;
            var right = instance.RightOperand;
            instance.returnType = left.ReturnType;
            if (right.ReturnType.FullName != parser.Primitives[Parser.Int].FullName)
                throw new TypeException(instance.SequencePoint, "Right shift operand must be of signed 32bit integer type");
            if (!left.ReturnType.IsIntegerType())
                throw new TypeException(instance.SequencePoint, "Left shift operand must be of integer type");
        }
        private static void ParseBinary(Parser parser, BinaryOperatorNode instance)
        {
            var left = instance.LeftOperand;
            var right = instance.RightOperand;
            instance.returnType = left.ReturnType;

            if (!(left.ReturnType.IsIntegerType() && right.ReturnType.IsIntegerType()))
                throw new TypeException(instance.SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.ReturnType, right.ReturnType);

            if(left.ReturnType.GetIntegerWidth() != right.ReturnType.GetIntegerWidth())
                throw new TypeException(instance.SequencePoint, "Binary operations only allowed on equal length integers, operands: {0}, {1}",
                    left.ReturnType, right.ReturnType);
        }
        private static void ParseLogical(Parser parser, BinaryOperatorNode instance)
        {
            var left = instance.LeftOperand;
            var right = instance.RightOperand;
            instance.returnType = parser.Primitives[Parser.Bool];

            if (!(left.ReturnType.IsBooleanType() && right.ReturnType.IsBooleanType()))
                throw new TypeException(instance.SequencePoint, "Logical operations only allowed on booleans, operands: {0}, {1}",
                    left.ReturnType, right.ReturnType);
        }
        public override string ToString()
        {
            return String.Format("(BinaryOp: {0} {1} {2})", LeftOperand, BinaryOperatorType, RightOperand);
        }
        public static Dictionary<string, BinaryOperatorNodeType> Operators;
        static BinaryOperatorNode()
        {
            Operators = new Dictionary<string, BinaryOperatorNodeType>();
            Operators["+"]  = BinaryOperatorNodeType.Addition;
            Operators["-"]  = BinaryOperatorNodeType.Subtraction;
            Operators["*"]  = BinaryOperatorNodeType.Multiplication;
            Operators["/"]  = BinaryOperatorNodeType.Division;
            Operators["%"]  = BinaryOperatorNodeType.Modulus;
            Operators["|"]  = BinaryOperatorNodeType.BinaryOr;
            Operators["&"]  = BinaryOperatorNodeType.BinaryAnd;
            Operators["^"]  = BinaryOperatorNodeType.BinaryXor;
            Operators[">"]  = BinaryOperatorNodeType.GreaterThan;
            Operators[">="] = BinaryOperatorNodeType.GreaterEqualThan;
            Operators["<"]  = BinaryOperatorNodeType.LessThan;
            Operators["<="] = BinaryOperatorNodeType.LessEqualThan;
            Operators["=="] = BinaryOperatorNodeType.Equals;
            Operators["!="] = BinaryOperatorNodeType.NotEquals;
            Operators["||"] = BinaryOperatorNodeType.LogicalOr;
            Operators["&&"] = BinaryOperatorNodeType.LogicalAnd;
            Operators[">>"] = BinaryOperatorNodeType.ShiftRight;
            Operators["<<"] = BinaryOperatorNodeType.ShiftLeft;
        }
    }
}
