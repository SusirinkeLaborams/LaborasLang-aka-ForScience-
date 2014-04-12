using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    class Parser
    {
        public AssemblyRegistry Registry { get; private set; }
        public CodeBlockNode Root { get; set; }
        private ByteInputIterator source;
        private Dictionary<string, TypeReference> primitives;
        public Parser(AssemblyRegistry registry, AstNode tree, ByteInputIterator source)
        {
            Registry = registry;
            this.source = source;
            ParserNode.Parse(this, null, tree);
        }
        public string GetNodeValue(AstNode node)
        {
            return Encoding.UTF8.GetString(source.Text(node.Token.Start, node.Token.End));
        }
    }
}
