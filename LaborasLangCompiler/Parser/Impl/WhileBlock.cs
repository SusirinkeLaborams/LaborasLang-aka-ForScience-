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
            if (instance.condition.TypeWrapper.FullName != parser.Bool.FullName)
                throw new TypeException(instance.SequencePoint, "Condition must be a boolean expression");
            instance.block = CodeBlockNode.Parse(parser, parent, lexerNode.Children[4]);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(WhileBlock: Condition: {0}, Block: {1}", Condition, ExecutedBlock);
        }
    }
}
