using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
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
        private Dictionary<string, LValueNode> symbols;
        private CodeBlockNode parent;
        public CodeBlockNode(CodeBlockNode parent) 
        {
            nodes = new List<IParserNode>();
            symbols = new Dictionary<string, LValueNode>();
            this.parent = parent;
        }

        public LValueNode GetSymbol(string name)
        {
            if(symbols.ContainsKey(name))
            {
                return symbols[name];
            }
            else
            {
                if (parent != null)
                    return parent.GetSymbol(name);
                else
                    return null;
            }
        }

        public ILValueNode AddSymbol(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                return null;
            symbols.Add(name, new LocalVariableNode(new VariableDefinition(name, type)));
            return symbols[name];
        }

        public static new CodeBlockNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            var instance = new CodeBlockNode(parent);
            foreach(var node in lexerNode.Children)
            {
                instance.nodes.Add(ParserNode.Parse(parser, instance, node));
            }
            return instance;
        }
    }
}
