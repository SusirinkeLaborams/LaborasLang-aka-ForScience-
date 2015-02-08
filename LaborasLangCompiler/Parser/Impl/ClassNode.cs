using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools.Types;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ParserNode, Context
    {
        #region fields
        private List<FieldDeclarationNode> fields;
        private List<FunctionDeclarationNode> declaredMethods;
        private List<FunctionDeclarationNode> lambdas;
        private List<Namespace> globalImports;
        private ClassNode parent;
        private Parser parser;
        private int lambdaCounter = 0;
        private AstNode lexerNode;
        #endregion fields

        #region properties

        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeEmitter TypeEmitter { get; private set; }
        public string FullName { get; private set; }
        public TypeReference TypeReference { get { return TypeEmitter.Get(parser.Assembly); } }

        #endregion properties

        public ClassNode(Parser parser, ClassNode parent, AstNode lexerNode) : base(parser.GetSequencePoint(lexerNode))
        {
            if (parser.Root == null)
                parser.Root = this;
            this.lexerNode = lexerNode;
            this.parent = parent;
            this.parser = parser;
            this.declaredMethods = new List<FunctionDeclarationNode>();
            this.lambdas = new List<FunctionDeclarationNode>();
            fields = new List<FieldDeclarationNode>();
            globalImports = new List<Namespace>();
            FullName = parser.Filename;
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }

        #region type wrapper

        public FieldReference GetField(string name)
        {
            return AssemblyRegistry.GetField(parser.Assembly, TypeEmitter, name);
        }

        public IEnumerable<MethodReference> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(parser.Assembly, TypeEmitter, name);
        }

        public TypeReference GetContainedType(string name)
        {
            return null; 
        }

        #endregion typewrapper

        #region context

        public ClassNode GetClass() 
        { 
            return this;
        }

        public FunctionDeclarationNode GetMethod() 
        {
            return null;
        }

        public ExpressionNode GetSymbol(string name, Context scope, SequencePoint point)
        {
            var field = GetField(name);
            if (field != null)
                return new FieldNode(null, field, scope, point);

            var methods = GetMethods(name);
            if (scope.IsStaticContext())
                methods = methods.Where(m => m.IsStatic());

            if (methods.Count() > 0)
                return AmbiguousMethodNode.Create(parser, methods, scope, null, point);

            var type = GetContainedType(name);
            if (type != null)
                return new TypeNode(type, scope, point);

            type = FindType(name, point);
            if(type != null)
                return new TypeNode(type, scope, point);

            var namespaze = FindNamespace(name, point);
            if (namespaze != null)
                return new NamespaceNode(namespaze, point);

            if (parent != null)
                return parent.GetSymbol(name, scope, point);

            return null;
        }

        public bool IsStaticContext()
        {
            //used while parsing types
            return false;
        }

        #endregion context

        #region type/namespace lookup

        public TypeReference FindType(string name, SequencePoint point)
        {
            //local types not implemented

            //primitives
            if (parser.IsPrimitive(name))
                return parser.GetPrimitive(name);

            //imports
            var types = globalImports.Select(namespaze => namespaze.GetContainedType(name)).Where(t => t != null);
            if(types.Count() != 0)
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

            if (parent == null)
            {
                return parser.ProjectParser.FindType(name);
            }

            return null;
        }

        public Namespace FindNamespace(string name, SequencePoint point)
        {
            Namespace namespaze = null;

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

            if (namespaze == null && parent == null)
            {
                namespaze = parser.ProjectParser.FindNamespace(name);
            }

            return namespaze;
        }

        public void AddImport(NamespaceNode namespaze, SequencePoint point)
        {
            if (globalImports.Any(n => n.Name == namespaze.Namespace.Name))
                ErrorCode.DuplicateImport.ReportAndThrow(point, "Namespace {0} already imported", namespaze);

            globalImports.Add(namespaze.Namespace);
        }

        #endregion type/namespace lookup

        #region parsing

        public void ParseDeclarations()
        {
            foreach (var node in lexerNode.Children)
            {
                try
                {
                    switch (node.Type)
                    {
                        case Lexer.TokenType.UseNode:
                            ImportNode.Parse(parser, this, node);
                            break;
                        case Lexer.TokenType.DeclarationNode:
                            ParseDeclaration(node);
                            break;
                        default:
                            ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(node), "Import or declaration expected, {0} received", node.Type);
                            break;//unreachable
                    }
                }
                catch (CompilerException) { }//recover, continue
            }
        }

        private void ParseDeclaration(AstNode lexerNode)
        {
            var declaration = DeclarationInfo.Parse(parser, lexerNode);

            if(!declaration.Initializer.IsNull && declaration.Initializer.IsFunctionDeclaration() && !declaration.Modifiers.HasFlag(Modifiers.Mutable))
            {
                //method
                var method = FunctionDeclarationNode.ParseAsMethod(parser, this, declaration);
                declaredMethods.Add(method);
            }
            else
            {
                //field
                var field = new FieldDeclarationNode(parser, this, declaration, parser.GetSequencePoint(lexerNode));
                fields.Add(field);
            }
        }

        public void ParseInitializers()
        {
            foreach(var field in fields)
            {
                try
                {
                    field.Initialize(parser);
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
