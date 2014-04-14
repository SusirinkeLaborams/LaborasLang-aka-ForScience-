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
        protected RootNode(Parser parser) : base(null)
        {
            fileClass = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        public override Tree.ILValueNode AddSymbol(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                return null;
            var field = new FieldNode(null, new FieldDefinition(name, FieldAttributes.Private | FieldAttributes.Static, type));
            fileClass.AddField(field.Field);
            symbols.Add(name, field);
            return field;
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

        /*private static void ParseDeclaration(Parser parser, Dictionary<string, LValueNode> table, AstNode lexerNode)
        {

            var declaredType = parser.ParseType(lexerNode.Children[0]);
            var name = parser.GetNodeValue(lexerNode.Children[1]);

        }

        public static new SymbolDeclarationNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            ILValueNode symbol = null;
            IExpressionNode initializer = null;
            string type = lexerNode.Token.Name;
            if (type == "Declaration" || type == "DeclarationAndAssignment")
            {
                try
                {
                    var declaredType = parser.ParseType(lexerNode.Children[0]);
                    var name = parser.GetNodeValue(lexerNode.Children[1]);
                    symbol = parent.AddSymbol(declaredType, name);
                    if (type == "DeclarationAndAssignment")
                        initializer = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
                }
                catch (Exception e)
                {
                    throw new ParseException("Failed to parse declaration " + parser.GetNodeValue(lexerNode), e);
                }
            }
            else
            {
                throw new ParseException("Declaration expected, " + lexerNode.Token.Name + " received");
            }
            return new SymbolDeclarationNode(symbol, initializer);
        }*/
    }
}
