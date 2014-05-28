using LaborasLangCompiler.LexingTools;
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

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperatorNode : RValueNode, IUnaryOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.UnaryOperator; } }
        public override TypeReference ReturnType  { get; set; }
        public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
        public IExpressionNode Operand { get; private set; }
        private UnaryOperatorNode(UnaryOperatorNodeType type, IExpressionNode operand)
            : base(operand.SequencePoint)
        {
            Operand = operand;
            UnaryOperatorType = type;
        }
        public static new IExpressionNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            if(lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            }
            else
            {
                switch(lexerNode.Token.Name)
                {
                    case Lexer.SuffixNode:
                        return ParseSuffix(parser, parent, lexerNode);
                    case Lexer.PrefixNode:
                        return ParsePrefix(parser, parent, lexerNode);
                    default:
                        throw new ParseException("Unary op node expected, " + lexerNode.Token.Name + " received");
                }
            }
        }
        private static IExpressionNode ParseSuffix(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            var ops = new List<UnaryOperatorNodeType>();
            for (int i = 1; i < lexerNode.Children.Count; i++ )
            {
                string op = parser.ValueOf(lexerNode.Children[i]);
                try
                {
                    ops.Add(SuffixOperators[op]);
                }
                catch(KeyNotFoundException)
                {
                    throw new ParseException(String.Format("Suffix op expected, '{0}' received", op));
                }
            }
            return ParseUnary(parser, expression, ops);
        }
        private static IExpressionNode ParsePrefix(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var count = lexerNode.Children.Count;
            var expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[count - 1]);
            var ops = new List<UnaryOperatorNodeType>();
            for (int i = count - 2; i >= 0; i--)
            {
                string op = parser.ValueOf(lexerNode.Children[i]);
                try
                {
                    ops.Add(PrefixOperators[op]);
                }
                catch (KeyNotFoundException)
                {
                    throw new ParseException(String.Format("Prefix op expected, '{0}' received", op));
                }
            }
            return ParseUnary(parser, expression, ops);
        }
        private static IExpressionNode ParseUnary(Parser parser, IExpressionNode expression, List<UnaryOperatorNodeType> ops)
        {
            foreach(var op in ops)
            {
                expression = ParseUnary(parser, expression, op);
            }
            return expression;
        }
        private static UnaryOperatorNode ParseUnary(Parser parser, IExpressionNode expression, UnaryOperatorNodeType op)
        {
            var instance = new UnaryOperatorNode(op, expression);
            instance.ReturnType = expression.ReturnType;
            switch(op)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    instance.ParseBinary(parser);
                    break;
                case UnaryOperatorNodeType.LogicalNot:
                    instance.ParseLogical(parser);
                    break;
                case UnaryOperatorNodeType.Negation:
                    instance.ParseNegation(parser);
                    break;
                case UnaryOperatorNodeType.PostDecrement:
                case UnaryOperatorNodeType.PostIncrement:
                case UnaryOperatorNodeType.PreDecrement:
                case UnaryOperatorNodeType.PreIncrement:
                    instance.ParseInc(parser);
                    break;
                default:
                    throw new ParseException("Unary op expected, " + op + " received");
            }
            return instance;
        }
        private void ParseInc(Parser parser)
        {
            if (!ReturnType.IsNumericType() || Operand is LiteralNode)
                throw new TypeException(String.Format("Increment/Decrement ops only allowed on numeric typed variables, {0} received",
                    ReturnType));
        }
        private void ParseNegation(Parser parser)
        {
            if (!ReturnType.IsNumericType())
                throw new TypeException(String.Format("Arithmetic ops only allowed on numeric types, {0} received",
                    ReturnType));
        }
        private void ParseLogical(Parser parser)
        {
            if (!ReturnType.IsBooleanType())
                throw new TypeException(String.Format("Logical ops only allowed on boolean types, {0} received",
                    ReturnType));
        }
        private void ParseBinary(Parser parser)
        {
            if (!ReturnType.IsIntegerType())
                throw new TypeException(String.Format("Binary ops only allowed on integer types, {0} received",
                    ReturnType));
        }
        public static UnaryOperatorNode Void(IExpressionNode expression)
        {
            return new UnaryOperatorNode(UnaryOperatorNodeType.VoidOperator, expression);
        }
        public override string ToString()
        {
            return String.Format("(UnaryOp: {0} {1})", UnaryOperatorType, Operand);
        }
        public static Dictionary<string, UnaryOperatorNodeType> SuffixOperators;
        public static Dictionary<string, UnaryOperatorNodeType> PrefixOperators;
        static UnaryOperatorNode()
        {
            SuffixOperators = new Dictionary<string, UnaryOperatorNodeType>();
            PrefixOperators = new Dictionary<string, UnaryOperatorNodeType>();

            SuffixOperators["++"] = UnaryOperatorNodeType.PostIncrement;
            SuffixOperators["--"] = UnaryOperatorNodeType.PostDecrement;

            PrefixOperators["++"] = UnaryOperatorNodeType.PreIncrement;
            PrefixOperators["--"] = UnaryOperatorNodeType.PreDecrement;
            PrefixOperators["-"] = UnaryOperatorNodeType.Negation;
            PrefixOperators["!"] = UnaryOperatorNodeType.LogicalNot;
            PrefixOperators["~"] = UnaryOperatorNodeType.BinaryNot;
        }
    }
}
