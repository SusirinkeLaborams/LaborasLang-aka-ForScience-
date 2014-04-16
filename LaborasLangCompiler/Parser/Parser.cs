using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl;
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
        public AssemblyEmitter Assembly { get; private set; }
        public ClassNode Root { get; set; }
        public string Filename { get; private set; }
        private ByteInputIterator source;
        public IReadOnlyDictionary<string, TypeReference> Primitives { get; private set; }
        public Parser(AssemblyEmitter assembly, AstNode tree, ByteInputIterator source, string filename)
        {
            Assembly = assembly;
            this.source = source;
            Filename = filename;

            var primitives = new Dictionary<string, TypeReference>();
            primitives.Add("bool", AssemblyRegistry.GetType("System.Boolean"));
            primitives.Add("int", AssemblyRegistry.GetType("System.Int32"));
            primitives.Add("float", AssemblyRegistry.GetType("System.Single"));
            primitives.Add("string", AssemblyRegistry.GetType("System.String"));
            primitives.Add("void", AssemblyRegistry.GetType("System.Void"));
            primitives.Add("auto", null);
            Primitives = primitives;

            ClassNode.Parse(this, null, null, tree);
            Assembly.Save();
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
        /// <summary>
        /// Parses node as a type
        /// </summary>
        /// <param name="typeNode">The node to parse</param>
        /// <returns>Mono.Cecil.TypeReference</returns>
        /// <exception cref="TypeException">If the type is not a .NET primitive</exception>
        public TypeReference ParseType(AstNode typeNode)
        {
            if(typeNode.Children.Count == 1)
            {
                string type = GetNodeValue(typeNode.Children[0]);
                if (Primitives.ContainsKey(type))
                    return Primitives[type];
                else
                    throw new TypeException("Type " + type + " is not a primitive .NET type");
            }
            else
            {
                throw new NotImplementedException("Only parsing primitives");
            }
        }
        public static bool CompareTypes(TypeReference first, TypeReference second)
        {
            if (first.Name != second.Name)
                return false;
            if (first.Namespace != second.Namespace)
                return false;
            string firstName = first.Scope is ModuleDefinition ? first.Module.Assembly.Name.FullName : first.FullName;
            string secondName = second.Scope is ModuleDefinition ? second.Module.Assembly.Name.FullName : second.FullName;
            if (firstName != secondName)
                return false;
            return true;
        }
    }
}
