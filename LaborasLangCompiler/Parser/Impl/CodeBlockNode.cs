using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
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
            if (symbols.ContainsKey(name))
                return symbols[name];

            //check parent block table
            if (parent != null)
                return parent.GetSymbol(name);

            //symbol not found
            return null;
        }
        public virtual ILValueNode AddVariable(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                return null;
            symbols.Add(name, new LocalVariableNode(new VariableDefinition(name, type)));
            return symbols[name];
        }
        private void AddNode(ParserNode node)
        {
            nodes.Add(node);
        }
        private void AddExpression(ExpressionNode node, Parser parser)
        {
            if (node.ReturnType == parser.Primitives[Parser.Void])
                AddNode(node);
            else
                AddNode(UnaryOperatorNode.Void(node));
        }
        public static CodeBlockNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode, IReadOnlyList<FunctionArgumentNode> args = null)
        {
            var instance = new CodeBlockNode(parentBlock);
            if (args != null)
                instance.nodes.AddRange(args);
            foreach (var node in lexerNode.Children)
            {
                if (node.Token.Name == Lexer.Sentence)
                {
                    var sentence = node.Children[0];
                    switch (sentence.Token.Name)
                    {
                        case Lexer.NamespaceImport:
                            throw new ParseException("Imports only allowed in classes");
                        case Lexer.Declaration:
                        case Lexer.DeclarationAndAssignment:
                            instance.AddNode(SymbolDeclarationNode.Parse(parser, parentClass, instance, sentence));
                            break;
                        case Lexer.Assignment:
                            instance.AddExpression(AssignmentOperatorNode.Parse(parser, parentClass, instance, sentence), parser);
                            break;
                        case Lexer.FunctionCall:
                        case Lexer.Loop:
                        case Lexer.ConditionalSentence:
                            throw new NotImplementedException();
                        case Lexer.CodeBlock:
                            instance.AddNode(CodeBlockNode.Parse(parser, parentClass, instance, sentence));
                            break;
                        default:
                            throw new ParseException("Node " + sentence.Token.Name + " in sentence, dafuq");
                    }
                }
                else
                {
                    throw new ParseException("Sentence expected, " + node.Token.Name + " received");
                }
            }
            return instance;
        }
    }
}
