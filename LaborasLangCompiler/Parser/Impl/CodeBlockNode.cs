using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;
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
    class CodeBlockNode : ContextNode, ICodeBlockNode, IReturningNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        public bool Returns { get; private set; }

        private List<ParserNode> nodes;
        private Dictionary<string, SymbolDeclarationNode> symbols;

        private CodeBlockNode(ContextNode parent, SequencePoint point) : base(parent.Parser, parent, point)
        {
            nodes = new List<ParserNode>();
            symbols = new Dictionary<string, SymbolDeclarationNode>();
            Returns = false;
        }

        public override ClassNode GetClass()
        {
            return Parent.GetClass();
        }

        public override FunctionDeclarationNode GetMethod()
        {
            return Parent.GetMethod();
        }

        public override bool IsStaticContext()
        {
            return Parent.IsStaticContext();
        }

        public override ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
            {
                SymbolDeclarationNode node = symbols[name];
                return new LocalVariableNode(point, node.Variable, node.IsConst);
            }

            return Parent.GetSymbol(name, scope, point);
        }

        private void AddVariable(SymbolDeclarationNode variable)
        {
            if (symbols.ContainsKey(variable.Variable.Name))
            {
                ErrorCode.SymbolAlreadyDeclared.ReportAndThrow(variable.SequencePoint,
                    String.Format("Variable {0} already declared in this scope", variable.Variable.Name));
            }
            symbols.Add(variable.Variable.Name, variable);
        }

        private void AddDeclaration(SymbolDeclarationNode node)
        {
            AddVariable(node);
            nodes.Add(node);
        }

        private void AddExpression(ExpressionNode node, Parser parser)
        {
            if (node.ExpressionReturnType.TypeEquals(parser.Void))
                nodes.Add(node);
            else
                nodes.Add(UnaryOperatorNode.Void(node));
        }

        public CodeBlockNode AddNode(Parser parser, ParserNode node)
        {
            var returning = node as IReturningNode;
            if (returning != null && returning.Returns)
                Returns = true;

            var expression = node as ExpressionNode;
            if(expression != null)
            {
                AddExpression(expression, parser);
                return this;
            }

            var declaration = node as SymbolDeclarationNode;
            if(declaration != null)
            {
                AddDeclaration(declaration);
                return this;
            }

            // no special action
            if(node is CodeBlockNode || node is ConditionBlockNode || node is WhileBlock || node is ReturnNode)
            {
                nodes.Add(node);
                return this;
            }

            ErrorCode.InvalidStructure.ReportAndThrow(node.SequencePoint, "Unexpected node {0} in while parsing code block", node.GetType().FullName);
            return null;//unreachable
        }

        private ParserNode ParseNode(Parser parser, AstNode lexerNode)
        {
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.DeclarationNode:
                    return SymbolDeclarationNode.Parse(parser, this, lexerNode);
                case Lexer.TokenType.Value:
                    return ExpressionNode.Parse(parser, this, lexerNode);
                case Lexer.TokenType.WhileLoop:
                    return WhileBlock.Parse(parser, this, lexerNode);
                case Lexer.TokenType.ConditionalSentence:
                    return ConditionBlockNode.Parse(parser, this, lexerNode);
                case Lexer.TokenType.CodeBlockNode:
                    return CodeBlockNode.Parse(parser, this, lexerNode);
                case Lexer.TokenType.ReturnNode:
                    return ReturnNode.Parse(parser, this, lexerNode);
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(lexerNode), "Unexpected node {0} in while parsing code block", lexerNode.Type);
                    return null;//unreachable
            }
        }

        public static CodeBlockNode Parse(Parser parser, ContextNode parent, AstNode lexerNode)
        {
            CodeBlockNode instance = null;
            if(lexerNode.Type == Lexer.TokenType.CodeBlockNode)
            {
                instance = new CodeBlockNode(parent, parser.GetSequencePoint(lexerNode));
                foreach(var node in lexerNode.Children)
                {
                    try
                    {
                        switch (node.Type)
                        {
                            case Lexer.TokenType.LeftCurlyBrace:
                            case Lexer.TokenType.RightCurlyBrace:
                            case Lexer.TokenType.EndOfLine:
                                break;
                            default:
                                instance.AddNode(parser, instance.ParseNode(parser, node));
                                break;
                        }
                    }
                    catch (CompilerException) { }//recover, continue parsing
                }
            }
            else if(lexerNode.Type == Lexer.TokenType.StatementNode)
            {
                instance = new CodeBlockNode(parent, parser.GetSequencePoint(lexerNode));
                instance.AddNode(parser, instance.ParseNode(parser, lexerNode.Children[0]));
            }
            return instance;
        }

        public static CodeBlockNode Create(ContextNode parent, SequencePoint point)
        {
            return new CodeBlockNode(parent, point);
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
