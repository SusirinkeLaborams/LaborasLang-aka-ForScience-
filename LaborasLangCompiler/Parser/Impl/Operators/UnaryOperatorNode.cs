using LaborasLangCompiler.Common;
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
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperatorNode : ExpressionNode, IUnaryOperatorNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.UnaryOperator; } }
        public override TypeReference ExpressionReturnType { get { return operand.ExpressionReturnType; } }
        public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
        public IExpressionNode Operand { get { return operand; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get 
            { 
                return UnaryOperatorType == UnaryOperatorNodeType.PreDecrement || 
                    UnaryOperatorType == UnaryOperatorNodeType.PreIncrement;
            }
        }

        private ExpressionNode operand;

        private UnaryOperatorNode(UnaryOperatorNodeType type, ExpressionNode operand)
            : base(operand.SequencePoint)
        {
            this.operand = operand;
            this.UnaryOperatorType = type;
        }

        public static ExpressionNode Parse(ContextNode context, AstNode lexerNode)
        {
            if(lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(context, lexerNode.Children[0]);
            }
            else
            {
                switch(lexerNode.Type)
                {
                    case Lexer.TokenType.PostfixNode:
                        return ParseSuffix(context, lexerNode);
                    case Lexer.TokenType.PrefixNode:
                        return ParsePrefix(context, lexerNode);
                    default:
                        ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(lexerNode), "Unary op expected, {0} found", lexerNode.Type);
                        return null;//unreachable
                }
            }
        }

        private static ExpressionNode ParseSuffix(ContextNode context, AstNode lexerNode)
        {
            var expression = ExpressionNode.Parse(context, lexerNode.Children[0]);
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
                    ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(lexerNode.Children[i]), "Suffix op expected, '{0}' received", op);
                }
            }
            return Create(context, expression, ops);
        }

        private static ExpressionNode ParsePrefix(ContextNode context, AstNode lexerNode)
        {
            var count = lexerNode.Children.Count;
            var expression = ExpressionNode.Parse(context, lexerNode.Children[count - 1]);
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
                    ErrorCode.InvalidStructure.ReportAndThrow(context.Parser.GetSequencePoint(lexerNode.Children[i]), "Prefix op expected, '{0}' received", op);
                }
            }
            return Create(context, expression, ops);
        }

        private static ExpressionNode Create(ContextNode context, ExpressionNode expression, List<UnaryOperatorNodeType> ops)
        {
            foreach(var op in ops)
            {
                expression = Create(context, expression, op);
            }
            return expression;
        }

        public static UnaryOperatorNode Create(ContextNode context, ExpressionNode expression, UnaryOperatorNodeType op)
        {
            if(!expression.IsGettable)
            {
                ErrorCode.NotAnRValue.ReportAndThrow(expression.SequencePoint, "Unary operands must be gettable");
            }
            if(op != UnaryOperatorNodeType.BinaryNot && 
                op != UnaryOperatorNodeType.LogicalNot && 
                op != UnaryOperatorNodeType.Negation && 
                !expression.IsSettable)
            {
                ErrorCode.NotAnLValue.ReportAndThrow(expression.SequencePoint, "Unary operation {0} requires a settable operand", op);
            }
            var instance = new UnaryOperatorNode(op, expression);
            switch(op)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    instance.ParseBinary();
                    break;
                case UnaryOperatorNodeType.LogicalNot:
                    instance.ParseLogical();
                    break;
                case UnaryOperatorNodeType.Negation:
                    instance.ParseNegation();
                    break;
                case UnaryOperatorNodeType.PostDecrement:
                case UnaryOperatorNodeType.PostIncrement:
                case UnaryOperatorNodeType.PreDecrement:
                case UnaryOperatorNodeType.PreIncrement:
                    instance.ParseInc();
                    break;
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(expression.SequencePoint, "Unary op expected, '{0}' received", op);
                    break;//unreachable
            }
            return instance;
        }

        private void ParseInc()
        {
            if (!ExpressionReturnType.IsNumericType())
                ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint, "Increment/Decrement ops only allowed on numeric types, {0} received",
                    ExpressionReturnType);
        }

        private void ParseNegation()
        {
            if (!ExpressionReturnType.IsNumericType())
                ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint, "Negation operations only allowed on numeric types, {0} received",
                    ExpressionReturnType);
        }

        private void ParseLogical()
        {
            if (!ExpressionReturnType.IsBooleanType())
                ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint, "Logical ops only allowed on boolean types, {0} received",
                    ExpressionReturnType);
        }

        private void ParseBinary()
        {
            if (!ExpressionReturnType.IsIntegerType())
                ErrorCode.TypeMissmatch.ReportAndThrow(SequencePoint, "Binary ops only allowed on integer types, {0} received",
                    ExpressionReturnType);
        }

        public static UnaryOperatorNode Void(ExpressionNode expression)
        {
            return new UnaryOperatorNode(UnaryOperatorNodeType.VoidOperator, expression);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("UnaryOperator:");
            builder.Indent(indent + 1).AppendLine("Operator:");
            builder.Indent(indent + 2).AppendLine(UnaryOperatorType.ToString());
            builder.Indent(indent + 1).AppendLine("Operand:");
            builder.AppendLine(operand.ToString(indent + 2));
            return builder.ToString();
        }

        public static IReadOnlyDictionary<Lexer.TokenType, UnaryOperatorNodeType> SuffixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>()
        {
            
            {Lexer.TokenType.PlusPlus, UnaryOperatorNodeType.PostIncrement},
            {Lexer.TokenType.MinusMinus, UnaryOperatorNodeType.PostDecrement}
        };

        public static IReadOnlyDictionary<Lexer.TokenType, UnaryOperatorNodeType> PrefixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>()
        {
            {Lexer.TokenType.PlusPlus, UnaryOperatorNodeType.PreIncrement},
            {Lexer.TokenType.MinusMinus, UnaryOperatorNodeType.PreDecrement},
            {Lexer.TokenType.Minus, UnaryOperatorNodeType.Negation},
            {Lexer.TokenType.Not, UnaryOperatorNodeType.LogicalNot},
            {Lexer.TokenType.BitwiseComplement, UnaryOperatorNodeType.BinaryNot}
        };
    }
}
