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

namespace LaborasLangCompiler.Parser.Impl
{
    class UnaryOperators
    {
        public class VoidOperatorNode : ExpressionNode, IUnaryOperatorNode
        {
            public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.UnaryOperator; } }
            public override TypeReference ExpressionReturnType { get { return operand.ExpressionReturnType; } }
            public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
            public IExpressionNode Operand { get { return operand; } }
            public override bool IsGettable { get { return false; } }
            public override bool IsSettable { get { return false; } }

            private readonly ExpressionNode operand;

            public VoidOperatorNode(ExpressionNode operand)
                : base(operand.SequencePoint)
            {
                this.operand = operand;
                this.UnaryOperatorType = UnaryOperatorNodeType.VoidOperator;
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

        private enum InternalUnaryOperatorType
        {
            BinaryNot,
            LogicalNot,
            Negation,
            PreIncrement,
            PreDecrement,
            PostIncrement,
            PostDecrement
        }

        public class UnaryOperatorNode : ExpressionNode, IUnaryOperatorNode
        {
            public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.UnaryOperator; } }
            public override TypeReference ExpressionReturnType { get { return operand.ExpressionReturnType; } }
            public UnaryOperatorNodeType UnaryOperatorType { get; private set; }
            public IExpressionNode Operand { get { return operand; } }
            public override bool IsGettable { get { return true; } }
            public override bool IsSettable { get { return false; } }

            private readonly ExpressionNode operand;

            internal UnaryOperatorNode(UnaryOperatorNodeType type, ExpressionNode operand)
                : base(operand.SequencePoint)
            {
                this.operand = operand;
                this.UnaryOperatorType = type;
            }
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

        private static UnaryOperatorNode AsBuiltIn(ContextNode context, ExpressionNode expression, UnaryOperatorNodeType op)
        {
            var instance = new UnaryOperatorNode(op, expression);
            bool verified = false;
            switch (op)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    verified = instance.VerifyBinary();
                    break;
                case UnaryOperatorNodeType.LogicalNot:
                    verified = instance.VerifyLogical();
                    break;
                case UnaryOperatorNodeType.Negation:
                    verified = instance.VerifyNegation();
                    break;
                case UnaryOperatorNodeType.PostDecrement:
                case UnaryOperatorNodeType.PostIncrement:
                case UnaryOperatorNodeType.PreDecrement:
                case UnaryOperatorNodeType.PreIncrement:
                    verified = instance.VerifyInc();
                    break;
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(expression.SequencePoint, "Unary op expected, '{0}' received", op);
                    break;//unreachable
            }
            return verified ? instance : null;
        }

        private static ExpressionNode AsOverload(ContextNode context, ExpressionNode expression, UnaryOperatorNodeType op)
        {
            string name = Overloads[op];
            var point = expression.SequencePoint;
            var methods = TypeUtils.GetOperatorMethods(context.Assembly, expression, name);
            var args = expression.Enumerate();
            var argsTypes = args.Select(a => a.ExpressionReturnType).ToArray();

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
                        "Overloaded operator {0} for operand {1} is ambiguous",
                        name, expression.ExpressionReturnType.FullName);
                    return null;//unreachable
                }
            }
        }

        public static ExpressionNode Create(ContextNode context, ExpressionNode expression, UnaryOperatorNodeType op)
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

            ExpressionNode result = AsBuiltIn(context, expression, op);
            if (result == null)
                result = AsOverload(context, expression, op);

            if (result == null)
                OperatorMissmatch(expression.SequencePoint, op, expression.ExpressionReturnType);

            Contract.Assume(result != null);
            return result;
        }

        private ExpressionNode AsInc(ExpressionNode expression, InternalUnaryOperatorType op)
        {

        }

        private bool VerifyNegation()
        {
            return ExpressionReturnType.IsNumericType();
        }

        private bool VerifyLogical()
        {
            return ExpressionReturnType.IsBooleanType();
        }

        private bool VerifyBinary()
        {
            return ExpressionReturnType.IsIntegerType();
        }

        private static void OperatorMissmatch(SequencePoint point, UnaryOperatorNodeType op, TypeReference operand)
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(point,
                "Unable to perform {0} on operand {1}, no built int operation or operaror overload found",
                op, operand.FullName);
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

        public class IncrementDecrementOperatorNode : UnaryOperatorNode, IIncrementDecrementOperatorNode
        {

            public IncrementDecrementOperatorType IncrementDecrementType {get; private set;}

            public MethodReference OverloadedOperatorMethod {get; private set;}

            internal IncrementDecrementOperatorNode(IncrementDecrementOperatorType type, ExpressionNode operand, MethodReference overload)
                :base(UnaryOperatorNodeType.IncrementDecrement, operand)
            {
                IncrementDecrementType = type;
                OverloadedOperatorMethod = overload;
            }
        }

        [Pure]
        private static bool IsIncrementDecrement(UnaryOperatorNodeType op)
        {
            return op == UnaryOperatorNodeType.PostDecrement || op == UnaryOperatorNodeType.PostIncrement || op == UnaryOperatorNodeType.PreDecrement || op == UnaryOperatorNodeType.PreIncrement;
        }

        private static IReadOnlyDictionary<Lexer.TokenType, UnaryOperatorNodeType> SuffixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>()
        {
            
            {Lexer.TokenType.PlusPlus, UnaryOperatorNodeType.PostIncrement},
            {Lexer.TokenType.MinusMinus, UnaryOperatorNodeType.PostDecrement}
        };

        private static IReadOnlyDictionary<Lexer.TokenType, UnaryOperatorNodeType> PrefixOperators = new Dictionary<Lexer.TokenType, UnaryOperatorNodeType>()
        {
            {Lexer.TokenType.PlusPlus, UnaryOperatorNodeType.PreIncrement},
            {Lexer.TokenType.MinusMinus, UnaryOperatorNodeType.PreDecrement},
            {Lexer.TokenType.Minus, UnaryOperatorNodeType.Negation},
            {Lexer.TokenType.Not, UnaryOperatorNodeType.LogicalNot},
            {Lexer.TokenType.BitwiseComplement, UnaryOperatorNodeType.BinaryNot}
        };

        private static IReadOnlyDictionary<UnaryOperatorNodeType, string> Overloads = new Dictionary<UnaryOperatorNodeType, string>()
        {
            {UnaryOperatorNodeType.PostIncrement, "op_Increment"},
            {UnaryOperatorNodeType.PostDecrement, "op_Decrement"},
            {UnaryOperatorNodeType.PreIncrement, "op_Increment"},
            {UnaryOperatorNodeType.PreDecrement, "op_Decrement"},
            {UnaryOperatorNodeType.Negation, "op_UnaryNegation"},
            {UnaryOperatorNodeType.LogicalNot, "op_LogicalNot"},
            {UnaryOperatorNodeType.BinaryNot, "op_OnesComplement"}
        };
    }
}
