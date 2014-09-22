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
        private void AddNode(Parser parser, AstNode lexerNode)
        {
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.DeclarationNode:
                    AddNode(SymbolDeclarationNode.Parse(parser, this, lexerNode));
                    break;
                case Lexer.TokenType.Value:
                    AddExpression(ExpressionNode.Parse(parser, this, lexerNode), parser);
                    break;
                case Lexer.TokenType.WhileLoop:
                    AddNode(WhileBlock.Parse(parser, this, lexerNode));
                    break;
                case Lexer.TokenType.ConditionalSentence:
                    AddNode(ConditionBlockNode.Parse(parser, this, lexerNode));
                    break;
                case Lexer.TokenType.CodeBlockNode:
                    AddNode(CodeBlockNode.Parse(parser, this, lexerNode));
                    break;
                case Lexer.TokenType.Return:
                    AddNode(ReturnNode.Parse(parser, this, lexerNode));
                    break;
                default:
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Node " + lexerNode.Type + " in sentence, dafuq");
            }
        }
        public static CodeBlockNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            CodeBlockNode instance = null;
            if(lexerNode.Type == Lexer.TokenType.CodeBlockNode)
            {
                instance = new CodeBlockNode(parent, parser.GetSequencePoint(lexerNode));
                foreach(var node in lexerNode.Children)
                {
                    switch(node.Type)
                    {
                        case Lexer.TokenType.LeftCurlyBrace:
                        case Lexer.TokenType.RightCurlyBrace:
                        case Lexer.TokenType.EndOfLine:
                            break;
                        default:
                            instance.AddNode(parser, node);
                            break;
                    }
                }
            }
            else if(lexerNode.Type == Lexer.TokenType.StatementNode)
            {
                instance = new CodeBlockNode(parent, parser.GetSequencePoint(lexerNode));
                instance.AddNode(parser, lexerNode.Children[0]);
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
