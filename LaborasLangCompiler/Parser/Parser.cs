using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
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
        public AssemblyEmitter Assembly { get; private set; }
        public CodeBlockNode Root { get; set; }
        public string Filename { get; private set; }
        private ByteInputIterator source;
        public IReadOnlyDictionary<string, TypeReference> Primitives { get; private set; }
        public Parser(AssemblyEmitter assembly, AssemblyRegistry registry, AstNode tree, ByteInputIterator source, string filename)
        {
            Registry = registry;
            Assembly = assembly;
            this.source = source;
            Filename = filename;

            var primitives = new Dictionary<string, TypeReference>();
            primitives.Add("bool", Registry.GetType("System.Boolean"));
            primitives.Add("int", Registry.GetType("System.Int32"));
            primitives.Add("float", Registry.GetType("System.Single"));
            primitives.Add("string", Registry.GetType("System.String"));
            Primitives = primitives;

            RootNode.Parse(this, null, tree);
        }
        public string GetNodeValue(AstNode node)
        {
            return Encoding.UTF8.GetString(source.Text(node.Token.Start, node.Token.End));
        }
        public Dictionary<string, AstNode> FindChildren(string[] types, AstNode node)
        {
            Dictionary<string, AstNode> ret = new Dictionary<string, AstNode>();
            foreach(var type in types)
            {
                ret.Add(type, null);
            }
            foreach(var child in node.Children)
            {
                string type = child.Token.Name;
                if(ret.ContainsKey(type))
                {
                    if(ret[type] == null)
                        ret[type] = child;
                    else
                        throw new ParseException("Multiple definitions of " + type + " in node");
                }
            }
            return ret;
        }
        public TypeReference ParseType(AstNode typeNode)
        {
            if(typeNode.Children.Count == 1)
            {
                string type = GetNodeValue(typeNode.Children[0]);
                if (Primitives.ContainsKey(type))
                    return Primitives[type];
                else
                    throw new SymbolNotFoundException("Type " + type + " is not a primitive .NET type");
            }
            else
            {
                throw new NotImplementedException("Only parsing primitives");
            }
        }
    }
}
