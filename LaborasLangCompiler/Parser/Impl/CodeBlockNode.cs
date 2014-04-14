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
        protected List<IParserNode> nodes;
        protected Dictionary<string, LValueNode> symbols;
        private CodeBlockNode parent;
        protected CodeBlockNode(CodeBlockNode parent) 
        {
            nodes = new List<IParserNode>();
            symbols = new Dictionary<string, LValueNode>();
            this.parent = parent;
        }
        public LValueNode GetSymbol(string name)
        {
            //check node table
            if(symbols.ContainsKey(name))
                return symbols[name];

            //check parent block table
            if (parent != null)
                return parent.GetSymbol(name);

            //symbol not found
            return null;
        }

        public virtual ILValueNode AddSymbol(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                return null;
            symbols.Add(name, new LocalVariableNode(new VariableDefinition(name, type)));
            return symbols[name];
        }

        public static new CodeBlockNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new CodeBlockNode(parentBlock);
            foreach(var node in lexerNode.Children)
            {
                instance.nodes.Add(ParserNode.Parse(parser, parentClass, instance, node));
            }
            return instance;
        }
    }
}
