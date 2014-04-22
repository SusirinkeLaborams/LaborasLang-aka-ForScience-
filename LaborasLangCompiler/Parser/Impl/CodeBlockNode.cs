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
    class CodeBlockNode : ParserNode, ICodeBlockNode, IContainerNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        protected List<ParserNode> nodes;
        protected Dictionary<string, LValueNode> symbols;
        private IContainerNode parent;
        protected CodeBlockNode(IContainerNode parent)
        {
            nodes = new List<ParserNode>();
            symbols = new Dictionary<string, LValueNode>();
            this.parent = parent;
        }
        public ClassNode GetClass() { return parent.GetClass(); }
        public FunctionDeclarationNode GetFunction() { return parent.GetFunction(); }
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
        public virtual LValueNode AddVariable(TypeReference type, string name)
        {
            if (symbols.ContainsKey(name))
                throw new SymbolAlreadyDeclaredException(String.Format("Var {0} already declared", name));
            symbols.Add(name, new LocalVariableNode(new VariableDefinition(name, type)));
            return symbols[name];
        }
        private void AddNode(ParserNode node)
        {
            nodes.Add(node);
        }
        private void AddExpression(ExpressionNode node, Parser parser)
        {
            if (node.ReturnType.FullName == parser.Primitives[Parser.Void].FullName)
                AddNode(node);
            else
                AddNode(UnaryOperatorNode.Void(node));
        }
        public static CodeBlockNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new CodeBlockNode(parent);
            if (parent is FunctionDeclarationNode)
            {
                var function = (FunctionDeclarationNode)parent;
                foreach (var arg in function.Args)
                {
                    instance.symbols.Add(arg.Param.Name, arg);
                }
            }
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
                            instance.AddNode(SymbolDeclarationNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.Assignment:
                            instance.AddExpression(AssignmentOperatorNode.Parse(parser, instance, sentence), parser);
                            break;
                        case Lexer.FunctionCall:
                            instance.AddExpression(MethodCallNode.Parse(parser, instance, sentence), parser);
                            break;
                        case Lexer.Loop:
                            instance.AddNode(WhileBlock.Parse(parser, instance, sentence));
                            break;
                        case Lexer.ConditionalSentence:
                            instance.AddNode(ConditionBlockNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.CodeBlock:
                            instance.AddNode(CodeBlockNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.ReturnSentence:
                            instance.AddNode(ReturnNode.Parse(parser, instance, sentence));
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
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(CodeBlock: Symbols: (");
            string delim = "";
            foreach(var symbol in symbols)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, symbol.Value.ToString(), symbol.Key));
                delim = ", ";
            }
            delim = "";
            builder.Append(") Nodes: (");
            foreach(var node in nodes)
            {
                builder.Append(delim).Append(node.ToString());
                delim = ", ";
            }
            builder.Append("))");
            return builder.ToString();
        }
    }
}
