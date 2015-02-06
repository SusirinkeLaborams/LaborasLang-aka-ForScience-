using LaborasLangCompiler.Common;
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
    class CodeBlockNode : ParserNode, ICodeBlockNode, Context, ReturningNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        public bool Returns { get; private set; }

        protected List<ParserNode> nodes;
        protected Dictionary<string, SymbolDeclarationNode> symbols;
        private Context parent;


        protected CodeBlockNode(Context parent, SequencePoint point) : base(point)
        {
            nodes = new List<ParserNode>();
            symbols = new Dictionary<string, SymbolDeclarationNode>();
            this.parent = parent;
            Returns = false;
        }

        public ClassNode GetClass() { return parent.GetClass(); }

        public FunctionDeclarationNode GetMethod() { return parent.GetMethod(); }

        public ExpressionNode GetSymbol(string name, Context scope, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
            {
                SymbolDeclarationNode node = symbols[name];
                return new LocalVariableNode(point, node.Variable, node.IsConst);
            }

            return parent.GetSymbol(name, scope, point);
        }

        public bool IsStaticContext()
        {
            return parent.IsStaticContext();
        }

        public void AddVariable(SymbolDeclarationNode variable)
        {
            if (symbols.ContainsKey(variable.Variable.Name))
            {
                ErrorHandling.Report(ErrorCode.SymbolAlreadyDeclared, variable.SequencePoint,
                    String.Format("Variable {0} already declared in this scope", variable.Variable.Name));
            }
            symbols.Add(variable.Variable.Name, variable);
        }

        private void AddNode(SymbolDeclarationNode node)
        {
            AddVariable(node);
            nodes.Add(node);
        }

        private void AddNode(ParserNode node)
        {
            var nod = node as ReturningNode;
            if (nod != null && nod.Returns)
                Returns = true;
            nodes.Add(node);
        }

        private void AddExpression(ExpressionNode node, Parser parser)
        {
            if (Utils.TypesEqual(node.ExpressionReturnType, parser.Void))
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
                case Lexer.TokenType.ReturnNode:
                    AddNode(ReturnNode.Parse(parser, this, lexerNode));
                    break;
                default:
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Node " + lexerNode.Type + " in sentence, dafuq");
            }
        }

        public static CodeBlockNode Parse(Parser parser, Context parent, AstNode lexerNode)
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

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("CodeBlock:");
            builder.Indent(indent + 1).AppendLine("Symbols:");
            foreach(var symbol in symbols.Values)
            {
                builder.Indent(2 + indent).Append(symbol.GetSignature()).AppendLine();
            }
            builder.Indent(1 + indent).AppendLine("Nodes:");
            foreach(var node in nodes)
            {
                builder.AppendLine(node.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
