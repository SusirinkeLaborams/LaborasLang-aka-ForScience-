using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ConditionBlockNode : ParserNode, IConditionBlock, ReturningNode
    {
        public override NodeType Type { get { return NodeType.ConditionBlock; } }
        public IExpressionNode Condition { get { return condition; } }
        public ICodeBlockNode TrueBlock { get { return trueBlock; } }
        public ICodeBlockNode FalseBlock { get { return falseBlock; } }
        protected ConditionBlockNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public bool Returns
        {
            get
            {
                return ((CodeBlockNode)TrueBlock).Returns && FalseBlock != null && ((CodeBlockNode)FalseBlock).Returns;
            }
        }

        private ExpressionNode condition;
        private CodeBlockNode trueBlock, falseBlock;
        public static ConditionBlockNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var instance = new ConditionBlockNode(parser.GetSequencePoint(lexerNode));
            instance.condition = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            if (!instance.condition.TypeWrapper.IsAssignableTo(parser.Bool))
                throw new TypeException(instance.SequencePoint, "Condition must be a boolean expression");
            instance.trueBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[4]);
            if (lexerNode.Children.Count > 5)
                instance.falseBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[6]);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ConditionBlock: Condition: {0}, True: {1}, False: {2}", Condition, TrueBlock, FalseBlock);
        }
    }
}
