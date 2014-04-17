﻿using LaborasLangCompiler.ILTools;
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
            primitives.Add("bool", Assembly.TypeToTypeReference(typeof(bool)));
            primitives.Add("int", Assembly.TypeToTypeReference(typeof(int)));
            primitives.Add("float", Assembly.TypeToTypeReference(typeof(float)));
            primitives.Add("string", Assembly.TypeToTypeReference(typeof(string)));
            primitives.Add("void", Assembly.TypeToTypeReference(typeof(void)));
            primitives.Add("long", Assembly.TypeToTypeReference(typeof(long)));
            primitives.Add("auto", null);
            Primitives = primitives;

            ClassNode.Parse(this, null, null, tree);
        }
        public string ValueOf(AstNode node)
        {
            return Encoding.UTF8.GetString(source.Text(node.Token.Start, node.Token.End));
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
                string type = ValueOf(typeNode.Children[0]);
                if (Primitives.ContainsKey(type))
                    return Primitives[type];
                else
                    throw new TypeException("Type '" + type + "' is not a primitive .NET type");
            }
            else
            {
                throw new NotImplementedException("Only parsing primitives");
            }
        }
    }
}
