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
    class WhileBlock : ParserNode, IWhileBlockNode
    {
        public override NodeType Type { get { return NodeType.WhileBlock; } }
        public IExpressionNode Condition { get { return condition; } }
        public ICodeBlockNode ExecutedBlock { get { return block; } }

        private ExpressionNode condition;
        private CodeBlockNode block;
        protected WhileBlock(SequencePoint point) : base(point) { }
        public static WhileBlock Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var instance = new WhileBlock(parser.GetSequencePoint(lexerNode));
            instance.condition = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            if (instance.condition.TypeWrapper.FullName != parser.Bool.FullName || !instance.condition.IsGettable)
                throw new TypeException(instance.SequencePoint, "Condition must be a gettable boolean expression");
            instance.block = CodeBlockNode.Parse(parser, parent, lexerNode.Children[4]);
            return instance;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("While:");
            builder.Indent(indent + 1).AppendLine("Condition:");
            builder.AppendLine(condition.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Block:");
            builder.AppendLine(block.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
