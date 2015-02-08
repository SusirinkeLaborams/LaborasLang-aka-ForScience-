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

        public static AssignmentOperatorNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var instance = new AssignmentOperatorNode(parser.GetSequencePoint(lexerNode));
            var left = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            if (!left.IsSettable)
                ErrorCode.NotAnLValue.ReportAndThrow(left.SequencePoint, "Left of assignment operator must be settable");
            var right = ExpressionNode.Parse(parser, parent, lexerNode.Children[2], left.ExpressionReturnType);
            if(!right.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Right of assignment operator must be gettable");
            instance.type = left.ExpressionReturnType;

            //use properties from lexer instead of string comparisons here
            var op = lexerNode.Children[1];
            if (op.Type != Lexer.TokenType.Assignment)
            {
                if(!left.IsGettable)
                    ErrorCode.NotAnRValue.ReportAndThrow(right.SequencePoint, "Left of this type of assignment operator must be gettable");
                right = BinaryOperatorNode.Parse(parser, Operators[op.Type], left, right);
            }

            if (!right.ExpressionReturnType.IsAssignableTo(left.ExpressionReturnType))
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(instance.SequencePoint, 
                    "Cannot assign {0} to {1}", instance.right.ExpressionReturnType, instance.left.ExpressionReturnType);
            }
            instance.right = right;
            instance.left = left;
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

        public static Dictionary<Lexer.TokenType, BinaryOperatorNodeType> Operators = new Dictionary<Lexer.TokenType, BinaryOperatorNodeType>()
        {
            {Lexer.TokenType.PlusEqual, BinaryOperatorNodeType.Addition},
            {Lexer.TokenType.MinusEqual, BinaryOperatorNodeType.Subtraction},
            {Lexer.TokenType.MultiplyEqual, BinaryOperatorNodeType.Multiplication},
            {Lexer.TokenType.DivideEqual, BinaryOperatorNodeType.Division},
            {Lexer.TokenType.RemainderEqual, BinaryOperatorNodeType.Modulus},
            {Lexer.TokenType.BitwiseOrEqual, BinaryOperatorNodeType.BinaryOr},
            {Lexer.TokenType.BitwiseAndEqual, BinaryOperatorNodeType.BinaryAnd},
            {Lexer.TokenType.BitwiseXorEqual, BinaryOperatorNodeType.BinaryXor},
            {Lexer.TokenType.RightShiftEqual, BinaryOperatorNodeType.ShiftRight},
            {Lexer.TokenType.LeftShiftEqual, BinaryOperatorNodeType.ShiftLeft}
        };
    }
}
