using LaborasLangCompiler.Parser.Exceptions;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ConditionBlockNode : ParserNode, IConditionBlock
    {
        public override NodeType Type { get { return NodeType.ConditionBlock; } }
        public IExpressionNode Condition { get; private set; }
        public ICodeBlockNode TrueBlock { get; private set; }
        public ICodeBlockNode FalseBlock { get; private set; }
        public static ConditionBlockNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new ConditionBlockNode();
            instance.Condition = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0].Children[0]);
            if (instance.Condition.ReturnType.FullName != parser.Primitives[Parser.Bool].FullName)
                throw new TypeException("Condition must be a boolean expression");
            instance.TrueBlock = CodeBlockNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[1].Children[0]);
            if(lexerNode.Children.Count > 2)
                instance.FalseBlock = CodeBlockNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2].Children[0]);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ConditionBlock: Condition: {0}, True: {1}, False: {2}", Condition, TrueBlock, FalseBlock);
        }
    }
}
