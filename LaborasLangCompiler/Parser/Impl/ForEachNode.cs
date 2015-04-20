using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
using Lexer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ForEachNode : ContextNode, IForEachLoopNode
    {
        public override FunctionDeclarationNode GetMethod()
        {
            return Parent.GetMethod();
        }

        public override ClassNode GetClass()
        {
            return Parent.GetClass();
        }

        public override bool IsStaticContext()
        {
            return Parent.IsStaticContext();
        }

        public override ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point)
        {
            if(name == variable.Variable.Name)
            {
                return new LocalVariableNode(point, variable.Variable, true);
            }

            return Parent.GetSymbol(name, scope, point);
        }

        public override NodeType Type { get { return NodeType.ForEachLoop; } }
        public IExpressionNode Collection { get { return collection; } }
        public ISymbolDeclarationNode LoopVariable { get { return variable; } }
        public ICodeBlockNode Body { get { return body; } }

        private CodeBlockNode body;
        private LoopVariableDeclaration variable;
        private ExpressionNode collection;

        private ForEachNode(ContextNode context, SequencePoint point)
            :base(context.Parser, context, point)
        {
        }

        public static ForEachNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            //For + LeftParenthesis + ForEachDeclaration + In + Value + RightParenthesis + CodeConstruct),
            Contract.Requires(lexerNode.Type == Lexer.TokenType.ForEachLoop);

            var instance = new ForEachNode(context, context.Parser.GetSequencePoint(lexerNode));

            var collection = ExpressionNode.Parse(context, lexerNode.Children[4]);
            if(!collection.IsGettable)
            {
                ErrorCode.NotAnRValue.ReportAndThrow(collection.SequencePoint, "collection must be a gettable expression");
            }
            if(collection.ExpressionReturnType.IsTypeless())
            {
                ErrorCode.InvalidForEachCollection.ReportAndThrow(collection.SequencePoint, "collection must not be a typeless expression");
            }
            var collectionElementType = collection.ExpressionReturnType.GetEnumerableElementType();
            if(collectionElementType == null)
            {
                ErrorCode.InvalidForEachCollection.ReportAndThrow(collection.SequencePoint, "Cannot iterate over expression type {0}", collection.ExpressionReturnType);
            }

            var declaration = LoopVariableDeclaration.Parse(context, lexerNode.Children[2], collectionElementType);

            if(!collectionElementType.IsAssignableTo(declaration.Variable.VariableType))
            {
                ErrorCode.TypeMissmatch.ReportAndThrow(declaration.SequencePoint, "Cannot assign collection elements of type {0} to {1}", collectionElementType, declaration.Variable.VariableType);
            }

            instance.collection = collection;
            instance.variable = declaration;

            instance.body = CodeBlockNode.Parse(instance, lexerNode.Children[6]);

            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ForEach:");
            builder.Indent(indent + 1).AppendLine("Variable:");
            builder.AppendLine(variable.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Collection:");
            builder.AppendLine(collection.ToString(indent + 2));
            builder.Indent(indent + 1).AppendLine("Body:");
            builder.AppendLine(body.ToString(indent + 2));
            return builder.ToString();
        }

        internal class LoopVariableDeclaration : ParserNode, ISymbolDeclarationNode
        {
            public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
            public VariableDefinition Variable { get; private set; }

            public IExpressionNode Initializer
            {
                get { throw new InvalidOperationException(); }
            }

            private LoopVariableDeclaration(VariableDefinition variable, SequencePoint point)
                :base(point)
            {
                Variable = variable;
            }

            public static LoopVariableDeclaration Parse(ContextNode context, IAbstractSyntaxTree lexerNode, TypeReference collectionElementType)
            {
                Contract.Requires(lexerNode.Type == Lexer.TokenType.ForEachDeclaration);
                var declaredType = TypeNode.Parse(context, lexerNode.Children[0]);

                if (declaredType.IsVoid())
                {
                    ErrorCode.VoidLValue.ReportAndThrow(context.Parser.GetSequencePoint(lexerNode.Children[0]), "Cannot declare a variable of type void");
                }

                if (declaredType.IsAuto())
                {
                    declaredType = collectionElementType;
                }
                var name = AstUtils.GetSingleSymbolOrThrow(lexerNode.Children[1]);
                return new LoopVariableDeclaration(new VariableDefinition(name, declaredType), context.Parser.GetSequencePoint(lexerNode));
            }

            public override string ToString(int indent)
            {
                StringBuilder builder = new StringBuilder();
                builder.Indent(indent).AppendLine("VariableDeclaration:");
                builder.Indent(indent + 1).AppendLine("Symbol:");
                builder.Indent(indent + 2).AppendFormat("{0} {1}", Variable.VariableType, Variable.Name).AppendLine();
                return builder.ToString();
            }
        }
    }
}
