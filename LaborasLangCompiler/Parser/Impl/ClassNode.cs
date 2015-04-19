using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen.Types;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ContextNode
    {
        #region fields
        private readonly List<FieldDeclarationNode> fields;
        private readonly List<FunctionDeclarationNode> declaredMethods;
        private readonly List<FunctionDeclarationNode> lambdas;
        private readonly List<Namespace> globalImports;
        private int lambdaCounter = 0;
        #endregion fields

        #region properties

        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeEmitter TypeEmitter { get; private set; }
        public string FullName { get; private set; }
        public TypeReference TypeReference { get { return TypeEmitter.Get(Parser.Assembly); } }

        #endregion properties

        private ClassNode(Parser parser, ContextNode parent, SequencePoint point) : base(parser, parent, point)
        {
            this.declaredMethods = new List<FunctionDeclarationNode>();
            this.lambdas = new List<FunctionDeclarationNode>();
            fields = new List<FieldDeclarationNode>();
            globalImports = new List<Namespace>();
            FullName = parser.Filename;
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename, parser.Namespace.Name);

            if (!string.IsNullOrEmpty(parser.Namespace.Name))
                globalImports.Add(parser.Namespace);
        }

        public static ClassNode ForFile(Parser parser)
        {
            return new ClassNode(parser, null, null);
        }

        public static ClassNode ForGen(ContextNode context)
        {
            return new ClassNode(context.Parser, context.Parent, null);
        }

        #region type wrapper

        public FieldReference GetField(string name)
        {
            return AssemblyRegistry.GetField(Parser.Assembly, TypeEmitter, name);
        }

        public IEnumerable<MethodReference> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(Parser.Assembly, TypeEmitter, name);
        }

        public TypeReference GetContainedType(string name)
        {
            return null; 
        }

        #endregion typewrapper

        #region context

        public override ClassNode GetClass() 
        { 
            return this;
        }

        public override FunctionDeclarationNode GetMethod() 
        {
            return null;
        }

        public override ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point)
        {
            var field = GetField(name);
            if (field != null)
                return new FieldNode(null, field, scope, point);

            var methods = GetMethods(name);
            if (scope.IsStaticContext())
                methods = methods.Where(m => m.IsStatic());

            if (methods.Count() > 0)
                return AmbiguousMethodNode.Create(methods, scope, null, point);

            var type = GetContainedType(name);
            if (type != null)
                return TypeNode.Create(type, scope, point);

            var namespaze = GetImportedNamespace(name, point);
            if (namespaze != null)
                return new NamespaceNode(namespaze, point);

            type = GetImportedType(name, point);
            if(type != null)
                return TypeNode.Create(type, scope, point);

            if (Parent != null)
                return Parent.GetSymbol(name, scope, point);

            namespaze = Parser.ProjectParser.FindNamespace(name);
            if (namespaze != null)
                return new NamespaceNode(namespaze, point);

            type = Parser.ProjectParser.FindType(name);
            if (type != null)
                return TypeNode.Create(type, scope, point);

            return null;
        }

        public override bool IsStaticContext()
        {
            //used while parsing types
            return false;
        }

        #endregion context

        #region type/namespace lookup

        private TypeReference GetImportedType(string name, SequencePoint point)
        {
            var types = globalImports.Select(namespaze => namespaze.GetContainedType(name)).Where(t => t != null);
            if (types.Count() != 0)
            {
                if (types.Count() == 1)
                {
                    return types.Single();
                }
                else
                {
                    ErrorCode.AmbiguousSymbol.ReportAndThrow(point,
                        "Ambiguous type {0}, could be {1}", name, String.Join(", ", types.Select(t => t.FullName)));
                }
            }

            return null;
        }

        private Namespace GetImportedNamespace(string name, SequencePoint point)
        {
            var namespazes = globalImports.Select(import => import.GetContainedNamespace(name)).Where(n => n != null);
            if (namespazes.Count() != 0)
            {
                if (namespazes.Count() == 1)
                {
                    return namespazes.Single();
                }
                else
                {
                    ErrorCode.AmbiguousSymbol.ReportAndThrow(point,
                        "Ambiguous namespace {0}, could be {1}", name, String.Join(", ", namespazes.Select(t => t.Name)));
                }
            }

            return null;
        }

        public void AddImport(NamespaceNode namespaze, SequencePoint point)
        {
            if (globalImports.Any(n => n.Name == namespaze.Namespace.Name))
                ErrorCode.DuplicateImport.ReportAndThrow(point, "Namespace {0} already imported", namespaze);

            globalImports.Add(namespaze.Namespace);
        }

        #endregion type/namespace lookup

        #region parsing

        public void ParseDeclarations(IAbstractSyntaxTree lexerNode)
        {
            foreach (var node in lexerNode.Children)
            {
                try
                {
                    if (node.Type != Lexer.TokenType.StatementWithEndOfLine)
                    {
                        ErrorCode.InvalidStructure.ReportAndThrow(Parser.GetSequencePoint(node), "Import or declaration expected, {0} received", node.Type);
                    }
                    var statement = node.Children[0];
                    switch (statement.Type)
                    {
                        case Lexer.TokenType.UseNode:
                            ImportNode.Parse(this, statement);
                            break;
                        case Lexer.TokenType.DeclarationNode:
                            ParseDeclaration(statement);
                            break;
                        default:
                            ErrorCode.InvalidStructure.ReportAndThrow(Parser.GetSequencePoint(statement), "Import or declaration expected, {0} received", statement.Type);
                            break;//unreachable
                    }
                }
                catch (CompilerException) { }//recover, continue
            }
        }

        private void ParseDeclaration(IAbstractSyntaxTree lexerNode)
        {
            var declaration = DeclarationInfo.Parse(Parser, lexerNode);

            if(declaration.Initializer != null && declaration.Initializer.IsFunctionDeclaration() && !declaration.Modifiers.HasFlag(Modifiers.Mutable))
            {
                //method
                var method = FunctionDeclarationNode.ParseAsMethod(this, declaration);
                declaredMethods.Add(method);
            }
            else
            {
                //field
                var field = new FieldDeclarationNode(this, declaration, Parser.GetSequencePoint(lexerNode));
                fields.Add(field);
            }
        }

        public void ParseInitializers()
        {
            foreach(var field in fields)
            {
                try
                {
                    field.Initialize();
                }
                catch (CompilerException) { }//recover, continue
            }
        }

        public void AddLambda(FunctionDeclarationNode lambda)
        {
            lambdas.Add(lambda);
        }

        public void Emit()
        {
            foreach(var method in declaredMethods)
            {
                try
                {
                    method.Emit();
                }
                catch (CompilerException) { }//recover, continue
            }
        }

        #endregion parsing

        public string NewFunctionName()
        {
            return "$Lambda_" + lambdaCounter++.ToString();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Class:");
            builder.Indent(indent + 1).Append(FullName).AppendLine();
            builder.Indent(indent + 1).AppendLine("Fields:");
            foreach(var field in fields)
            {
                builder.AppendLine(field.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Methods:");
            foreach(var method in declaredMethods.Union(lambdas))
            {
                builder.AppendLine(method.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
