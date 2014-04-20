using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Operators;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class BinaryOperatorNode : RValueNode, IBinaryOperatorNode
    {
        public IExpressionNode RightOperand { get; set; }
        public IExpressionNode LeftOperand { get; set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.BinaryOperator; } }
        public BinaryOperatorNodeType BinaryOperatorType { get; set; }
        public override TypeReference ReturnType { get; set; }
        public static new ExpressionNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            }
            else
            {
                var op = parser.ValueOf(lexerNode.Children[1]);
                var left = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
                var right = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2]);
                return Parse(parser, op, left, right);
            }
        }
        public static BinaryOperatorNode Parse(Parser parser, string op, IExpressionNode left, IExpressionNode right)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                    return ArithmeticOperatorNode.Parse(parser, op, left, right);
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "==":
                case "!=":
                    return ComparisonOperatorNode.Parse(parser, op, left, right);
                case "<<":
                case ">>":
                    return ShiftOperatorNode.Parse(parser, op, left, right);
                default:
                    throw new ParseException(String.Format("Binary op expected, '{0}' received", op));
            }
        }
        public override string ToString()
        {
            return String.Format("(BinaryOp: {0} {1} {2})", LeftOperand, BinaryOperatorType, RightOperand);
        }
        public static Dictionary<string, BinaryOperatorNodeType> Operators;
        static BinaryOperatorNode()
        {
            Operators = new Dictionary<string, BinaryOperatorNodeType>();
            Operators["+"] = BinaryOperatorNodeType.Addition;
            Operators["-"] = BinaryOperatorNodeType.Subtraction;
            Operators["*"] = BinaryOperatorNodeType.Multiplication;
            Operators["/"] = BinaryOperatorNodeType.Division;
            Operators["%"] = BinaryOperatorNodeType.Modulus;
            Operators["|"] = BinaryOperatorNodeType.BinaryOr;
            Operators["&"] = BinaryOperatorNodeType.BinaryAnd;
            Operators["^"] = BinaryOperatorNodeType.Xor;
            Operators[">"] = BinaryOperatorNodeType.GreaterThan;
            Operators[">="] = BinaryOperatorNodeType.GreaterEqualThan;
            Operators["<"] = BinaryOperatorNodeType.LessThan;
            Operators["<="] = BinaryOperatorNodeType.LessEqualThan;
            Operators["=="] = BinaryOperatorNodeType.Equals;
            Operators["!="] = BinaryOperatorNodeType.NotEquals;
            Operators["||"] = BinaryOperatorNodeType.LogicalOr;
            Operators["&&"] = BinaryOperatorNodeType.LogicalAnd;
        }
    }
}
