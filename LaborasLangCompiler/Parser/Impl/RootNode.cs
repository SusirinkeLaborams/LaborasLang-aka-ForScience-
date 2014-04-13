using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
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
        private TypeEmitter fileClass;
        public RootNode(Parser parser) : base(null)
        {
            //fileClass = new TypeEmitter(parser.Registry, parser.Filename);
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
            var instance = new RootNode(parser);
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
                            goto case "DeclarationAndAssignment";
                        case "DeclarationAndAssignment":
                            instance.nodes.Add(SymbolDeclarationNode.Parse(parser, instance, sentence));
                            break;
                        default: 
                            throw new ParseException("Import or declaration expected " + sentence.Token.Name + " received");
                    }
                }
                else
                {
                    throw new ParseException("Node Sentence expected, " + node.Token.Name + " received");
                }
            }
            return instance;
        }
    }
}
