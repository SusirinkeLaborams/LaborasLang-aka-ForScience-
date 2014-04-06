using LaborasLangCompiler.Parser.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class CodeBlockNode : ParserNode, ICodeBlockNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        private List<IParserNode> nodes;
        private Dictionary<string, ILValueNode> symbols;
        public CodeBlockNode(CodeBlockNode parent) : base(parent)
        {
            nodes = new List<IParserNode>();
            symbols = new Dictionary<string, ILValueNode>();
        }

        public override ILValueNode GetSymbol(string name)
        {
            if (!symbols.ContainsKey(name))
                return base.GetSymbol(name);
            else
                return symbols[name];
        }
    }
}
