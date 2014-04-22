using LaborasLangCompiler.Parser.Exceptions;
using NPEG;
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
        public IExpressionNode Condition { get; private set; }
        public ICodeBlockNode ExecutedBlock { get; private set; }
        public static WhileBlock Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new WhileBlock();
            instance.Condition = ExpressionNode.Parse(parser, parent, lexerNode.Children[0].Children[0]);
            if (instance.Condition.ReturnType.FullName != parser.Primitives[Parser.Bool].FullName)
                throw new TypeException("Condition must be a boolean expression");
            instance.ExecutedBlock = CodeBlockNode.Parse(parser, parent, lexerNode.Children[1]);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(WhileBlock: Condition: {0}, Block: {1}", Condition, ExecutedBlock);
        }
    }
}
