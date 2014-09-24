using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class AssignmentOperatorNode : RValueNode, IAssignmentOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.AssignmentOperator; } }
        public override TypeWrapper TypeWrapper { get { return type; } }
        public ILValueNode LeftOperand { get { return left; } }
        public IExpressionNode RightOperand { get { return right; } }

        private TypeWrapper type;
        private LValueNode left;
        private ExpressionNode right;
        protected AssignmentOperatorNode(SequencePoint point) : base(point) { }
        public static new AssignmentOperatorNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var instance = new AssignmentOperatorNode(parser.GetSequencePoint(lexerNode));
            var left = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]) as LValueNode;
            if (left == null)
                throw new ParseException(left.SequencePoint, "LValue expected");
            var right = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            instance.type = left.TypeWrapper;

            //use properties from lexer instead of string comparisons here
            var op = lexerNode.Children[1];
            if (op.Type != Lexer.TokenType.Assignment)
                right = BinaryOperatorNode.Parse(parser, Operators[op.Type], left, right);

            if (right is AmbiguousNode)
                right = ((AmbiguousNode)right).RemoveAmbiguity(parser, left.TypeWrapper);

            if (!right.TypeWrapper.IsAssignableTo(left.TypeWrapper))
                throw new TypeException(instance.SequencePoint, "Assigned {0} to {1}", instance.right.TypeWrapper, instance.left.TypeWrapper);
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

        public static Dictionary<Lexer.TokenType, BinaryOperatorNodeType> Operators;
        static AssignmentOperatorNode()
        {
            Operators = new Dictionary<Lexer.TokenType, BinaryOperatorNodeType>();
            Operators[Lexer.TokenType.PlusEqual]  = BinaryOperatorNodeType.Addition;
            Operators[Lexer.TokenType.MinusEqual]  = BinaryOperatorNodeType.Subtraction;
            Operators[Lexer.TokenType.MultiplyEqual] = BinaryOperatorNodeType.Multiplication;
            Operators[Lexer.TokenType.DivideEqual]  = BinaryOperatorNodeType.Division;
            Operators[Lexer.TokenType.RemainderEqual]  = BinaryOperatorNodeType.Modulus;
            Operators[Lexer.TokenType.BitwiseOrEqual]  = BinaryOperatorNodeType.BinaryOr;
            Operators[Lexer.TokenType.BitwiseAndEqual]  = BinaryOperatorNodeType.BinaryAnd;
            Operators[Lexer.TokenType.BitwiseXorEqual]  = BinaryOperatorNodeType.BinaryXor;
            Operators[Lexer.TokenType.RightShiftEqual] = BinaryOperatorNodeType.ShiftRight;
            Operators[Lexer.TokenType.LeftShiftEqual] = BinaryOperatorNodeType.ShiftLeft;
        }
    }
}
