using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperatorNode : RValueNode, IUnaryOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.UnaryOperator; } }
        public override TypeReference ReturnType  { get; set; }
        public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
        public IExpressionNode Operand { get; private set; }
        private UnaryOperatorNode(UnaryOperatorNodeType type, IExpressionNode operand)
        {
            Operand = operand;
            UnaryOperatorType = type;
        }
        public static new ExpressionNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            if(lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            }
            else
            {
                switch(lexerNode.Token.Name)
                {
                    case Lexer.SuffixNode:
                        return ParseSuffix(parser, parentClass, parentBlock, lexerNode);
                    case Lexer.PrefixNode:
                        return ParsePrefix(parser, parentClass, parentBlock, lexerNode);
                    default:
                        throw new ParseException("Unary op node expected, " + lexerNode.Token.Name + " received");
                }
            }
        }
        private static ExpressionNode ParseSuffix(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var expression = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
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
        private static ExpressionNode ParsePrefix(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var count = lexerNode.Children.Count;
            var expression = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[count - 1]);
            var ops = new List<UnaryOperatorNodeType>();
            for (int i = count - 2; i <= 0; i--)
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
        private static ExpressionNode ParseUnary(Parser parser, IExpressionNode expression, List<UnaryOperatorNodeType> ops)
        {
            throw new NotImplementedException();
        }
        private static UnaryOperatorNode ParseArithmetic(Parser parser, IExpressionNode expression, UnaryOperatorNodeType op)
        {
            throw new NotImplementedException();
        }
        private static UnaryOperatorNode ParseLogical(Parser parser, IExpressionNode expression, UnaryOperatorNodeType op)
        {
            throw new NotImplementedException();
        }
        private static UnaryOperatorNode ParseBinary(Parser parser, IExpressionNode expression, UnaryOperatorNodeType op)
        {
            throw new NotImplementedException();
        }
        public static UnaryOperatorNode Void(ExpressionNode expression)
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
