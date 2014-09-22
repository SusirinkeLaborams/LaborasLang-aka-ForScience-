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
    class UnaryOperatorNode : RValueNode, IUnaryOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.UnaryOperator; } }
        public override TypeWrapper TypeWrapper { get { return operand.TypeWrapper; } }
        public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
        public IExpressionNode Operand { get { return operand; } }

        private ExpressionNode operand;
        private UnaryOperatorNode(UnaryOperatorNodeType type, ExpressionNode operand)
            : base(operand.SequencePoint)
        {
            this.operand = operand;
            this.UnaryOperatorType = type;
        }
        public static new ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if(lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            }
            else
            {
                switch(lexerNode.Type)
                {
                    case Lexer.TokenType.PostfixNode:
                        return ParseSuffix(parser, parent, lexerNode);
                    case Lexer.TokenType.PrefixNode:
                        return ParsePrefix(parser, parent, lexerNode);
                    default:
                        throw new ParseException(parser.GetSequencePoint(lexerNode), "Unary op node expected, " + lexerNode.Type + " received");
                }
            }
        }
        private static ExpressionNode ParseSuffix(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            var ops = new List<UnaryOperatorNodeType>();
            for (int i = 1; i < lexerNode.Children.Count; i++ )
            {
                var op = lexerNode.Children[i].Type;
                try
                {
                    ops.Add(SuffixOperators[op]);
                }
                catch(KeyNotFoundException)
                {
                    throw new ParseException(parser.GetSequencePoint(lexerNode.Children[i]), "Suffix op expected, '{0}' received", op);
                }
            }
            return ParseUnary(parser, expression, ops);
        }
        private static ExpressionNode ParsePrefix(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var count = lexerNode.Children.Count;
            var expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[count - 1]);
            var ops = new List<UnaryOperatorNodeType>();
            for (int i = count - 2; i >= 0; i--)
            {
                var op = lexerNode.Children[i].Type;
                try
                {
                    ops.Add(PrefixOperators[op]);
                }
                catch (KeyNotFoundException)
                {
                    throw new ParseException(parser.GetSequencePoint(lexerNode.Children[i]), "Prefix op expected, '{0}' received", op);
                }
            }
            return ParseUnary(parser, expression, ops);
        }
        private static ExpressionNode ParseUnary(Parser parser, ExpressionNode expression, List<UnaryOperatorNodeType> ops)
        {
            foreach(var op in ops)
            {
                expression = ParseUnary(parser, expression, op);
            }
            return expression;
        }
        private static UnaryOperatorNode ParseUnary(Parser parser, ExpressionNode expression, UnaryOperatorNodeType op)
        {
            var instance = new UnaryOperatorNode(op, expression);
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
                    throw new ParseException(null, "Unary op expected, " + op + " received");
            }
            return instance;
        }
        private void ParseInc(Parser parser)
        {
            if (!TypeWrapper.IsNumericType() || Operand is LiteralNode)
                throw new TypeException(SequencePoint, "Increment/Decrement ops only allowed on numeric typed variables, {0} received",
                    TypeWrapper);
        }
        private void ParseNegation(Parser parser)
        {
            if (!TypeWrapper.IsNumericType())
                throw new TypeException(SequencePoint, "Arithmetic ops only allowed on numeric types, {0} received",
                    TypeWrapper);
        }
        private void ParseLogical(Parser parser)
        {
            if (!TypeWrapper.IsBooleanType())
                throw new TypeException(SequencePoint, "Logical ops only allowed on boolean types, {0} received",
                    TypeWrapper);
        }
        private void ParseBinary(Parser parser)
        {
            if (!TypeWrapper.IsIntegerType())
                throw new TypeException(SequencePoint, "Binary ops only allowed on integer types, {0} received",
                    TypeWrapper);
        }
        public static UnaryOperatorNode Void(ExpressionNode expression)
        {
            return new UnaryOperatorNode(UnaryOperatorNodeType.VoidOperator, expression);
        }
        public override string ToString()
        {
            return String.Format("(UnaryOp: {0} {1})", UnaryOperatorType, Operand);
        }
        public static Dictionary<Lexer.TokenType, UnaryOperatorNodeType> SuffixOperators;
        public static Dictionary<Lexer.TokenType, UnaryOperatorNodeType> PrefixOperators;
        static UnaryOperatorNode()
        {
            SuffixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>();
            PrefixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>();

            SuffixOperators[Lexer.TokenType.PlusPlus] = UnaryOperatorNodeType.PostIncrement;
            SuffixOperators[Lexer.TokenType.MinusMinus] = UnaryOperatorNodeType.PostDecrement;

            PrefixOperators[Lexer.TokenType.PlusPlus] = UnaryOperatorNodeType.PreIncrement;
            PrefixOperators[Lexer.TokenType.MinusMinus] = UnaryOperatorNodeType.PreDecrement;
            PrefixOperators[Lexer.TokenType.Minus] = UnaryOperatorNodeType.Negation;
            PrefixOperators[Lexer.TokenType.Not] = UnaryOperatorNodeType.LogicalNot;
            PrefixOperators[Lexer.TokenType.BitwiseComplement] = UnaryOperatorNodeType.BinaryNot;
        }
    }
}
