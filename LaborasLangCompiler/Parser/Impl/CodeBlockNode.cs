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
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class CodeBlockNode : ContextNode, ICodeBlockNode, IReturningNode
    {
        public override NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes { get { return nodes; } }
        public bool Returns { get; private set; }

        private readonly List<ParserNode> nodes = new List<ParserNode>();
        private readonly Dictionary<string, SymbolDeclarationNode> symbols = new Dictionary<string, SymbolDeclarationNode>();

        private CodeBlockNode(ContextNode parent, SequencePoint point) : base(parent.Parser, parent, point)
        {
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

        private void AddExpression(ExpressionNode node)
        {
            nodes.Add(node);
        }

        public CodeBlockNode AddNode(ParserNode node)
        {
            var returning = node as IReturningNode;
            if (returning != null && returning.Returns)
                Returns = true;

            var expression = node as ExpressionNode;
            if(expression != null)
            {
                AddExpression(expression);
                return this;
            }

            var declaration = node as SymbolDeclarationNode;
            if(declaration != null)
            {
                AddDeclaration(declaration);
                return this;
            }

            // no special action
            if(node is CodeBlockNode || node is ConditionBlockNode || node is WhileBlock || node is ReturnNode || node is ForLoopNode)
            {
                nodes.Add(node);
                return this;
            }

            ContractsHelper.AssumeUnreachable("Unexpected node {0} in while parsing code block", node.GetType().FullName);
            return Utils.Utils.Fail<CodeBlockNode>();
        }

        public CodeBlockNode AddNode(IAbstractSyntaxTree lexerNode)
        {
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.WhileLoop:
                    return AddNode(WhileBlock.Parse(this, lexerNode));
                case Lexer.TokenType.ConditionalSentence:
                    return AddNode(ConditionBlockNode.Parse(this, lexerNode));
                case Lexer.TokenType.CodeBlockNode:
                    return AddNode(CodeBlockNode.Parse(this, lexerNode));
                case Lexer.TokenType.ForLoop:
                    return AddNode(ForLoopNode.Parse(this, lexerNode));
                case Lexer.TokenType.StatementWithEndOfLine:
                    if (lexerNode.Children.Count == 2)
                    {
                        return AddStatement(lexerNode.Children[0]);
                    }
                    else
                    {
                        //empty statement a.k.a ";"
                        return this;
                    }
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(Parser.GetSequencePoint(lexerNode), "Unexpected node {0} in while parsing code block", lexerNode.Type);
                    return null;//unreachable
            }
        }

        public CodeBlockNode AddStatement(IAbstractSyntaxTree lexerNode)
        {
            switch(lexerNode.Type)
            {
                case Lexer.TokenType.DeclarationNode:
                    return AddNode(SymbolDeclarationNode.Parse(this, lexerNode));
                case Lexer.TokenType.Value:
                    return AddNode(ExpressionNode.Parse(this, lexerNode));
                case Lexer.TokenType.ReturnNode:
                    return AddNode(ReturnNode.Parse(this, lexerNode));
                default:
                    ErrorCode.InvalidStructure.ReportAndThrow(Parser.GetSequencePoint(lexerNode), "Unexpected statement {0} in while parsing code block", lexerNode.Type);
                    return Utils.Utils.Fail<CodeBlockNode>();
            }
        }

        public static CodeBlockNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.CodeBlockNode || lexerNode.Type == Lexer.TokenType.StatementWithEndOfLine);
            CodeBlockNode instance = null;
            if(lexerNode.Type == Lexer.TokenType.CodeBlockNode)
            {
                instance = new CodeBlockNode(context, context.Parser.GetSequencePoint(lexerNode));
                foreach(var node in lexerNode.Children)
                {
                    try
                    {
                        switch (node.Type)
                        {
                            case Lexer.TokenType.LeftCurlyBrace:
                            case Lexer.TokenType.RightCurlyBrace:
                                break;
                            default:
                                instance.AddNode(node);
                                break;
                        }
                    }
                    catch (CompilerException) { }//recover, continue parsing
                }
            }
            else
            {
                instance = new CodeBlockNode(context, context.Parser.GetSequencePoint(lexerNode));
                instance.AddNode(lexerNode);
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
