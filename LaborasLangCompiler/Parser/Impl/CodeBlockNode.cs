using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class CodeBlockNode : ParserNode, ICodeBlockNode, ContainerNode, ReturningNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        public bool Returns { get; private set; }

        protected List<ParserNode> nodes;
        protected Dictionary<string, VariableWrapper> symbols;
        private ContainerNode parent;
        protected CodeBlockNode(ContainerNode parent, SequencePoint point) : base(point)
        {
            nodes = new List<ParserNode>();
            symbols = new Dictionary<string, VariableWrapper>();
            this.parent = parent;
            Returns = false;
        }
        public ClassNode GetClass() { return parent.GetClass(); }
        public FunctionDeclarationNode GetFunction() { return parent.GetFunction(); }
        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                return new LocalVariableNode(point, symbols[name]);

            return parent.GetSymbol(name, point);
        }
        public virtual LValueNode AddVariable(TypeWrapper type, string name, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                throw new SymbolAlreadyDeclaredException(point, "Var {0} already declared", name);
            symbols.Add(name, new VariableWrapper(name, type));
            return new LocalVariableNode(point, symbols[name]);
        }
        private void AddNode(ParserNode node)
        {
            if (node is ReturningNode)
                if (((ReturningNode)node).Returns)
                    Returns = true;
            nodes.Add(node);
        }
        private void AddExpression(ExpressionNode node, Parser parser)
        {
            if (node.TypeWrapper.FullName == parser.Void.FullName)
                AddNode(node);
            else
                AddNode(UnaryOperatorNode.Void(node));
        }
        public static CodeBlockNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var instance = new CodeBlockNode(parent, parser.GetSequencePoint(lexerNode));
            foreach (var node in lexerNode.Children)
            {
                if (node.Type == Lexer.TokenType.StatementNode)
                {
                    var sentence = node.Children[0];
                    switch (sentence.Type)
                    {
                        case Lexer.TokenType.DeclarationNode:
                            instance.AddNode(SymbolDeclarationNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.TokenType.FunctionCall:
                        case Lexer.TokenType.AssignmentNode:
                            instance.AddExpression(ExpressionNode.Parse(parser, instance, sentence), parser);
                            break;
                        case Lexer.TokenType.WhileLoop:
                            instance.AddNode(WhileBlock.Parse(parser, instance, sentence));
                            break;
                        case Lexer.TokenType.ConditionalSentence:
                            instance.AddNode(ConditionBlockNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.TokenType.CodeBlockNode:
                            instance.AddNode(CodeBlockNode.Parse(parser, instance, sentence));
                            break;
                        case Lexer.TokenType.Return:
                            instance.AddNode(ReturnNode.Parse(parser, instance, sentence));
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(sentence), "Node " + sentence.Type + " in sentence, dafuq");
                    }
                }
                else
                {
                    throw new ParseException(parser.GetSequencePoint(node), "Sentence expected, " + node.Type + " received");
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
                builder.Append(String.Format("{0}{1} {2}", delim, symbol.Value.TypeWrapper, symbol.Key));
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
