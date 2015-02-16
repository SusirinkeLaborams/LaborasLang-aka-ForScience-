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

        public static AssignmentOperatorNode Parse(ContextNode context, AstNode lexerNode)
        {
            var left = ExpressionNode.Parse(context, lexerNode.Children[0]);
            var right = ExpressionNode.Parse(context, lexerNode.Children[2], left.ExpressionReturnType);
            var point = context.Parser.GetSequencePoint(lexerNode);

            return Create(context, LexerToAssignemnt[lexerNode.Children[1].Type], left, right, point);    
        }

        public static AssignmentOperatorNode Create(ContextNode context, AssignmentOperatorType op, ExpressionNode left, ExpressionNode right, SequencePoint point)
        {
            if (!left.IsSettable)
                ErrorCode.NotAnLValue.ReportAndThrow(left.SequencePoint, "Left of assignment operator must be settable");

            if (!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Right of assignment operator must be gettable");

            var type = left.ExpressionReturnType;

            if(op != AssignmentOperatorType.Assignment)
            {
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

        public static Dictionary<Lexer.TokenType, AssignmentOperatorType> LexerToAssignemnt = new Dictionary<Lexer.TokenType, AssignmentOperatorType>()
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

        private static Dictionary<AssignmentOperatorType, BinaryOperatorNodeType> AssignmentToBinary = new Dictionary<AssignmentOperatorType, BinaryOperatorNodeType>()
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
