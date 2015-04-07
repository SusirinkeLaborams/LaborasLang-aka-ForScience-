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
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperators
    {
        public enum InternalUnaryOperatorType
        {
            BinaryNot,
            LogicalNot,
            Negation,
            PreIncrement,
            PreDecrement,
            PostIncrement,
            PostDecrement,
            VoidOperator
        }

        public class UnaryOperatorNode : ExpressionNode, IUnaryOperatorNode
        {
            public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.UnaryOperator; } }
            public override TypeReference ExpressionReturnType { get { return operand.ExpressionReturnType; } }
            public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
            public IExpressionNode Operand { get { return operand; } }
            public override bool IsGettable { get { return UnaryOperatorType != UnaryOperatorNodeType.VoidOperator; } }
            public override bool IsSettable { get { return false; } }

            private readonly ExpressionNode operand;

            internal UnaryOperatorNode(InternalUnaryOperatorType type, ExpressionNode operand)
                : base(operand.SequencePoint)
            {
                this.operand = operand;
                switch (type)
                {
                    case InternalUnaryOperatorType.BinaryNot:
                        this.UnaryOperatorType = UnaryOperatorNodeType.BinaryNot;
                        break;
                    case InternalUnaryOperatorType.LogicalNot:
                        this.UnaryOperatorType = UnaryOperatorNodeType.LogicalNot;
                        break;
                    case InternalUnaryOperatorType.Negation:
                        this.UnaryOperatorType = UnaryOperatorNodeType.Negation;
                        break;
                    case InternalUnaryOperatorType.VoidOperator:
                        this.UnaryOperatorType = UnaryOperatorNodeType.VoidOperator;
                        break;
                    default:
                        throw new ArgumentException();
                }
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
        }

        public class IncrementDecrementOperatorNode : ExpressionNode, IIncrementDecrementOperatorNode
        {
            public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.IncrementDecrementOperator; } }
            public override TypeReference ExpressionReturnType { get { return operand.ExpressionReturnType; } }
            public IExpressionNode Operand { get { return operand; } }
            public override bool IsGettable { get { return true; } }
            public override bool IsSettable
            { 
                get 
                {
                    return IncrementDecrementType == IncrementDecrementOperatorType.PreDecrement || IncrementDecrementType == IncrementDecrementOperatorType.PreIncrement; 
                }
            }

            public IncrementDecrementOperatorType IncrementDecrementType { get; private set; }
            public MethodReference OverloadedOperatorMethod { get; private set; }

            private ExpressionNode operand;

            internal IncrementDecrementOperatorNode(InternalUnaryOperatorType type, ExpressionNode operand, MethodReference overload)
                : base(operand.SequencePoint)
            {
                OverloadedOperatorMethod = overload;
                this.operand = operand;
                switch (type)
                {
                    case InternalUnaryOperatorType.PreIncrement:
                        IncrementDecrementType = IncrementDecrementOperatorType.PreIncrement;
                        break;
                    case InternalUnaryOperatorType.PreDecrement:
                        IncrementDecrementType = IncrementDecrementOperatorType.PreDecrement;
                        break;
                    case InternalUnaryOperatorType.PostIncrement:
                        IncrementDecrementType = IncrementDecrementOperatorType.PostIncrement;
                        break;
                    case InternalUnaryOperatorType.PostDecrement:
                        IncrementDecrementType = IncrementDecrementOperatorType.PostDecrement;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            public override string ToString(int indent)
            {
                StringBuilder builder = new StringBuilder();
                builder.Indent(indent).AppendLine("IncrementDecrementOperator:");
                builder.Indent(indent + 1).AppendLine("Operator:");
                builder.Indent(indent + 2).AppendLine(IncrementDecrementType.ToString());
                builder.Indent(indent + 1).AppendLine("Operand:");
                builder.AppendLine(operand.ToString(indent + 2));
                return builder.ToString();
            }
        }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            switch (lexerNode.Type)
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

        private static ExpressionNode ParseSuffix(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            var opType = lexerNode.Children[1].Type;
            switch (opType)
            {
                case Lexer.TokenType.PlusPlus:
                    return Create(context, ExpressionNode.Parse(context, lexerNode.Children[0]), InternalUnaryOperatorType.PostIncrement);
                case Lexer.TokenType.MinusMinus:
                    return Create(context, ExpressionNode.Parse(context, lexerNode.Children[0]), InternalUnaryOperatorType.PostDecrement);
                case Lexer.TokenType.FunctionArgumentsList:
                    return MethodCallNode.Parse(context, lexerNode);
                case Lexer.TokenType.IndexNode:
                    return ArrayAccessNode.Parse(context, lexerNode);
                default:
                    ContractsHelper.AssertUnreachable("Unknown postfix op {0}", opType);
                    return null;//unreachable
            }
        }

        private static ExpressionNode ParsePrefix(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            var opType = lexerNode.Children[1].Type;
            InternalUnaryOperatorType op;
            switch (opType)
            {
                case Lexer.TokenType.PlusPlus:
                    op = InternalUnaryOperatorType.PreIncrement;
                    break;
                case Lexer.TokenType.MinusMinus:
                    op = InternalUnaryOperatorType.PreDecrement;
                    break;
                case Lexer.TokenType.Not:
                    op = InternalUnaryOperatorType.LogicalNot;
                    break;
                case Lexer.TokenType.BitwiseComplement:
                    op = InternalUnaryOperatorType.BinaryNot;
                    break;
                case Lexer.TokenType.Minus:
                    op = InternalUnaryOperatorType.Negation;
                    break;
                default:
                    ContractsHelper.AssertUnreachable("Unknown prefix op {0}", opType);
                    return null;//unreachable
            }
            return Create(context, ExpressionNode.Parse(context, lexerNode.Children[0]), op);
        }

        private static ExpressionNode AsBuiltIn(ContextNode context, ExpressionNode expression, InternalUnaryOperatorType op)
        {
            ExpressionNode instance = null;
            switch (op)
            {
                case InternalUnaryOperatorType.BinaryNot:
                    instance = AsBinary(expression, op);
                    break;
                case InternalUnaryOperatorType.LogicalNot:
                    instance = AsLogical(expression, op);
                    break;
                case InternalUnaryOperatorType.Negation:
                    instance = AsNegation(expression, op);
                    break;
                case InternalUnaryOperatorType.PostDecrement:
                case InternalUnaryOperatorType.PostIncrement:
                case InternalUnaryOperatorType.PreDecrement:
                case InternalUnaryOperatorType.PreIncrement:
                    instance = AsInc(expression, op);
                    break;
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(expression.SequencePoint, "Unary op expected, '{0}' received", op);
                    break;//unreachable
            }
            return instance;
        }

        private static ExpressionNode AsOverload(ContextNode context, ExpressionNode expression, InternalUnaryOperatorType op)
        {
            string name = Overloads[op];
            var point = expression.SequencePoint;
            var methods = TypeUtils.GetOperatorMethods(context.Assembly, expression, name);
            var argsTypes = expression.ExpressionReturnType.Enumerate();

            methods = methods.Where(m => MetadataHelpers.MatchesArgumentList(m, argsTypes));

            var method = AssemblyRegistry.GetCompatibleMethod(methods, argsTypes);

            if (method != null)
            {
                return CreateOverload(context, expression, op, method);
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
                        "Overloaded operator {0} for operand {1} is ambiguous",
                        name, expression.ExpressionReturnType.FullName);
                    return null;//unreachable
                }
            }
        }

        private static ExpressionNode CreateOverload(ContextNode context, ExpressionNode expression, InternalUnaryOperatorType op, MethodReference method)
        {
            var point = expression.SequencePoint;
            if(IsIncrementDecrement(op))
            {
                TypeUtils.VerifyAccessible(method, context.GetClass().TypeReference, point);
                return new IncrementDecrementOperatorNode(op, expression, method);
            }
            else
            {
                return MethodCallNode.Create(context, new MethodNode(method, null, context, point), expression.Enumerate(), point);
            }
        }

        public static ExpressionNode Create(ContextNode context, ExpressionNode expression, InternalUnaryOperatorType op)
        {
            if(!expression.IsGettable)
            {
                ErrorCode.NotAnRValue.ReportAndThrow(expression.SequencePoint, "Unary operands must be gettable");
            }
            if(!expression.IsSettable && IsIncrementDecrement(op))
            {
                ErrorCode.NotAnLValue.ReportAndThrow(expression.SequencePoint, "Unary operation {0} requires a settable operand", op);
            }

            ExpressionNode result = AsBuiltIn(context, expression, op);
            if (result == null)
                result = AsOverload(context, expression, op);

            if (result == null)
                OperatorMissmatch(expression.SequencePoint, op, expression.ExpressionReturnType);

            Contract.Assume(result != null);
            return result;
        }

        public static ExpressionNode Void(ExpressionNode expression)
        {
            return new UnaryOperatorNode(InternalUnaryOperatorType.VoidOperator, expression);
        }

        private static ExpressionNode AsInc(ExpressionNode expression, InternalUnaryOperatorType op)
        {
            return expression.ExpressionReturnType.IsNumericType() ? new IncrementDecrementOperatorNode(op, expression, null) : null;
        }

        private static ExpressionNode AsNegation(ExpressionNode expression, InternalUnaryOperatorType op)
        {
            return expression.ExpressionReturnType.IsNumericType() ? new UnaryOperatorNode(op, expression) : null;
        }

        private static ExpressionNode AsLogical(ExpressionNode expression, InternalUnaryOperatorType op)
        {
            return expression.ExpressionReturnType.IsBooleanType() ? new UnaryOperatorNode(op, expression) : null;
        }

        private static ExpressionNode AsBinary(ExpressionNode expression, InternalUnaryOperatorType op)
        {
            return expression.ExpressionReturnType.IsIntegerType() ? new UnaryOperatorNode(op, expression) : null;
        }

        private static void OperatorMissmatch(SequencePoint point, InternalUnaryOperatorType op, TypeReference operand)
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(point,
                "Unable to perform {0} on operand {1}, no built int operation or operaror overload found",
                op, operand.FullName);
        }

        private static bool IsIncrementDecrement(InternalUnaryOperatorType op)
        {
            switch (op)
            {
                case InternalUnaryOperatorType.BinaryNot:
                case InternalUnaryOperatorType.LogicalNot:
                case InternalUnaryOperatorType.Negation:
                case InternalUnaryOperatorType.VoidOperator:
                    return false;
                case InternalUnaryOperatorType.PreIncrement:
                case InternalUnaryOperatorType.PreDecrement:
                case InternalUnaryOperatorType.PostIncrement:
                case InternalUnaryOperatorType.PostDecrement:
                    return true;
                default:
                    throw new ArgumentException();
            }
        }

        private static IReadOnlyDictionary<InternalUnaryOperatorType, string> Overloads = new Dictionary<InternalUnaryOperatorType, string>()
        {
            {InternalUnaryOperatorType.PostIncrement, "op_Increment"},
            {InternalUnaryOperatorType.PostDecrement, "op_Decrement"},
            {InternalUnaryOperatorType.PreIncrement, "op_Increment"},
            {InternalUnaryOperatorType.PreDecrement, "op_Decrement"},
            {InternalUnaryOperatorType.Negation, "op_UnaryNegation"},
            {InternalUnaryOperatorType.LogicalNot, "op_LogicalNot"},
            {InternalUnaryOperatorType.BinaryNot, "op_OnesComplement"}
        };
    }
}
