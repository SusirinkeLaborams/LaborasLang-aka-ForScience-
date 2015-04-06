using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Utils;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class AssignmentOperatorNode : ExpressionNode, IAssignmentOperatorNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.AssignmentOperator; } }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode LeftOperand { get { return left; } }
        public IExpressionNode RightOperand { get { return right; } }
        public override bool IsSettable
        {
            get { return false; }
        }
        public override bool IsGettable
        {
            get { return true; }
        }

        private TypeReference type;
        private ExpressionNode left;
        private ExpressionNode right;

        protected AssignmentOperatorNode(SequencePoint point) : base(point) { }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            var left = ExpressionNode.Parse(context, lexerNode.Children[0]);
#warning fix, first need to check type
            var right = ExpressionNode.Parse(context, lexerNode.Children[1], left.ExpressionReturnType);
            var point = context.Parser.GetSequencePoint(lexerNode);

            return Create(context, LexerToAssignemnt[lexerNode.Children[2].Type], left, right, point);    
        }

        public static ExpressionNode Create(ContextNode context, AssignmentOperatorType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            if (!left.IsSettable)
                ErrorCode.NotAnLValue.ReportAndThrow(left.SequencePoint, "Left of assignment operator must be settable");

            if (!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Right of assignment operator must be gettable");

            var type = left.ExpressionReturnType;

            var overloaded = AsOverloadedMethod(context, op, left, right, point);
            if (overloaded != null)
                return overloaded;

            if(op != AssignmentOperatorType.Assignment)
            {
                //only transform primitives
                if (!(context.Parser.IsPrimitive(left.ExpressionReturnType) && context.Parser.IsPrimitive(right.ExpressionReturnType)))
                    AssignmentMissmatch(left, right, point);

                if (!left.IsGettable)
                    ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Left of this type of assignment operator must be gettable");

                right = BinaryOperatorNode.Create(context, AssignmentToBinary[op], left, right, point);
            }

            if (!right.ExpressionReturnType.IsAssignableTo(left.ExpressionReturnType))
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(point,
                    "Cannot assign {0} to {1}", right.ExpressionReturnType, left.ExpressionReturnType);
            }

            var instance = new AssignmentOperatorNode(point);
            instance.left = left;
            instance.right = right;
            instance.type = left.ExpressionReturnType;
            
            return instance;
        }

        private static ExpressionNode AsOverloadedMethod(ContextNode context, AssignmentOperatorType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            if (!OperatorMethods.ContainsKey(op))
                return null;

            string name = OperatorMethods[op];
            return BinaryOperatorNode.AsOverload(context, name, left, right, point);
        }

        private static void AssignmentMissmatch(ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            ErrorCode.TypeMissmatch.ReportAndThrow(point,
                    "Cannot assign {0} to {1}", right.ExpressionReturnType, left.ExpressionReturnType);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Assignment:");
            builder.Indent(indent + 1).AppendLine("Left:");
            builder.AppendLine(left.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Right:");
            builder.AppendLine(right.ToString(indent + 2));
            return builder.ToString();
        }

        public enum AssignmentOperatorType
        {
            Assignment,
            AdditionAssignment,
            MinusAssignment,
            MultiplyAssignment,
            DivisionAssignment,
            ModulusAssignment,
            BinaryOrAssignment,
            BinaryAndAssignment,
            BinaryXorAssignment,
            ShiftRightAssignment,
            ShiftLeftAssignment
        }

        public static IReadOnlyDictionary<AssignmentOperatorType, string> OperatorMethods = new Dictionary<AssignmentOperatorType, string>()
        {
            {AssignmentOperatorType.AdditionAssignment, "op_AdditionAssignment"},
            {AssignmentOperatorType.MinusAssignment, "op_SubtractionAssignment"},
            {AssignmentOperatorType.MultiplyAssignment, "op_MultiplicationAssignment"},
            {AssignmentOperatorType.DivisionAssignment, "op_DivisionAssignment"},
            {AssignmentOperatorType.ModulusAssignment, "op_ModulusAssignment"},
            {AssignmentOperatorType.BinaryOrAssignment, "op_BitwiseOrAssignment"},
            {AssignmentOperatorType.BinaryAndAssignment, "op_BitwiseAndAssignment"},
            {AssignmentOperatorType.BinaryXorAssignment, "op_ExclusiveOrAssignment"},
            {AssignmentOperatorType.ShiftRightAssignment, "op_RightShiftAssignment"},
            {AssignmentOperatorType.ShiftLeftAssignment, "op_LeftShiftAssignment"}
        };

        public static IReadOnlyDictionary<Lexer.TokenType, AssignmentOperatorType> LexerToAssignemnt = new Dictionary<Lexer.TokenType, AssignmentOperatorType>()
        {
            {Lexer.TokenType.Assignment, AssignmentOperatorType.Assignment},
            {Lexer.TokenType.PlusEqual, AssignmentOperatorType.AdditionAssignment},
            {Lexer.TokenType.MinusEqual, AssignmentOperatorType.MinusAssignment},
            {Lexer.TokenType.MultiplyEqual, AssignmentOperatorType.MultiplyAssignment},
            {Lexer.TokenType.DivideEqual, AssignmentOperatorType.DivisionAssignment},
            {Lexer.TokenType.RemainderEqual, AssignmentOperatorType.ModulusAssignment},
            {Lexer.TokenType.BitwiseOrEqual, AssignmentOperatorType.BinaryOrAssignment},
            {Lexer.TokenType.BitwiseAndEqual, AssignmentOperatorType.BinaryAndAssignment},
            {Lexer.TokenType.BitwiseXorEqual, AssignmentOperatorType.BinaryXorAssignment},
            {Lexer.TokenType.RightShiftEqual, AssignmentOperatorType.ShiftRightAssignment},
            {Lexer.TokenType.LeftShiftEqual, AssignmentOperatorType.ShiftLeftAssignment}
        };

        private static IReadOnlyDictionary<AssignmentOperatorType, BinaryOperatorNodeType> AssignmentToBinary = new Dictionary<AssignmentOperatorType, BinaryOperatorNodeType>()
        {
            {AssignmentOperatorType.AdditionAssignment, BinaryOperatorNodeType.Addition},
            {AssignmentOperatorType.MinusAssignment, BinaryOperatorNodeType.Subtraction},
            {AssignmentOperatorType.MultiplyAssignment, BinaryOperatorNodeType.Multiplication},
            {AssignmentOperatorType.DivisionAssignment, BinaryOperatorNodeType.Division},
            {AssignmentOperatorType.ModulusAssignment, BinaryOperatorNodeType.Modulus},
            {AssignmentOperatorType.BinaryOrAssignment, BinaryOperatorNodeType.BinaryOr},
            {AssignmentOperatorType.BinaryAndAssignment, BinaryOperatorNodeType.BinaryAnd},
            {AssignmentOperatorType.BinaryXorAssignment, BinaryOperatorNodeType.BinaryXor},
            {AssignmentOperatorType.ShiftRightAssignment, BinaryOperatorNodeType.ShiftRight},
            {AssignmentOperatorType.ShiftLeftAssignment, BinaryOperatorNodeType.ShiftLeft}
        };
    }
}
