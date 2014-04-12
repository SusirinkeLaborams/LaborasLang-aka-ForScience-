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
            primitives = new Dictionary<string, TypeReference>();
            primitives.Add("bool", Registry.GetType("System.Boolean"));
            primitives.Add("int", Registry.GetType("System.Int32"));
            primitives.Add("float", Registry.GetType("System.Single"));
        }
        public string GetNodeValue(AstNode node)
        {
            return Encoding.UTF8.GetString(source.Text(node.Token.Start, node.Token.End));
        }
        public Dictionary<string, List<AstNode>> FindChildren(string[] types, AstNode node)
        {
            Dictionary<string, List<AstNode>> ret = new Dictionary<string,List<AstNode>>();
            foreach(var type in types)
            {
                ret.Add(type, new List<AstNode>());
            }
            foreach(var child in node.Children)
            {
                if(ret.ContainsKey(child.Token.Name))
                {
                    ret[child.Token.Name].Add(child);
                }
            }
            return ret;
        }
        public TypeReference ParseType(AstNode typeNode)
        {
            if(typeNode.Children.Count == 1)
            {
                string type = GetNodeValue(typeNode.Children[0]);
                if (primitives.ContainsKey(type))
                    return primitives[type];
                else
                    throw new NotImplementedException("Darius said he'll fix it later");
                    //return null;
            }
            else
            {
                throw new NotImplementedException("Only parsing primitives");
            }
        }
    }
}
