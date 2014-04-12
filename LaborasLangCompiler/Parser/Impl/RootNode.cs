﻿using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class RootNode : CodeBlockNode
    {
        public RootNode() : base(null)
        {
        }
        public override Tree.ILValueNode AddSymbol(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                return null;
            symbols.Add(name, new FieldNode(null, new FieldDefinition(name, FieldAttributes.Private | FieldAttributes.Static, type)));
            return symbols[name];
        }

        public static new CodeBlockNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            var instance = new RootNode();
            parser.Root = instance;
            foreach(var node in lexerNode.Children)
            {
                if(node.Token.Name == "Sentence")
                {
                    var sentence = node.Children[0];
                    switch(sentence.Token.Name)
                    {
                        case "NamespaceImport":
                            throw new NotImplementedException();
                        case "Declaration":
                            throw new NotImplementedException();
                        case "DeclarationAndAssignment":
                            throw new NotImplementedException();
                        default: 
                            throw new ParseException("Import or declaration expected " + sentence.Token.Name + " received");
                    }
                }
                else
                {
                    throw new ParseException("Node Sentence expected, " + node.Token.Name + " received");
                }
            }
            throw new NotImplementedException();
        }
    }
}