using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
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
        private Dictionary<string, InternalField> fields;
        private List<FunctionDeclarationNode> declaredMethods;
        private List<NamespaceWrapper> globalImports;
        private ClassNode parent;
        private Parser parser;
        private Dictionary<string, FunctionDeclarationNode> methods;
        private int lambdaCounter = 0;
        #endregion fields

        #region properties

        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeEmitter TypeEmitter { get; private set; }
        public string FullName { get; private set; }
        public TypeReference TypeReference { get { return TypeEmitter.Get(parser.Assembly); } }
        public bool IsStatic { get; private set; }

        #endregion properties

        public ClassNode(Parser parser, ClassNode parent, SequencePoint point) : base(point)
        {
            if (parser.Root == null)
                parser.Root = this;
            this.parent = parent;
            this.parser = parser;
            this.declaredMethods = new List<FunctionDeclarationNode>();
            this.methods = new Dictionary<string, FunctionDeclarationNode>();
            fields = new Dictionary<string, InternalField>();
            globalImports = new List<NamespaceWrapper>();
            FullName = parser.Filename;
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        #region type wrapper

        public FieldWrapper GetField(string name)
        {
            if (fields.ContainsKey(name))
                return fields[name];

            return null;
        }

        public IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return methods.Where(kv => kv.Key == name).Select(kv => kv.Value);
        }

        public MethodWrapper GetMethod(string name)
        {
            if (methods.ContainsKey(name))
                return methods[name];
            else
                return null;
        }

        public TypeWrapper GetContainedType(string name)
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

        public ExpressionNode GetSymbol(string name, TypeReference scope, SequencePoint point)
        {
            var field = GetField(name);
            if (field != null)
                return new FieldNode(null, field, scope, point);

            if (parent != null)
                return parent.GetSymbol(name, scope, point);

            return null;
        }

        public bool IsStaticContext()
        {
            throw new InvalidOperationException("ClassNode is not a complete execution context");
        }

        #endregion context

        #region type/namespace lookup

        public TypeNode FindType(string name, TypeReference scope, SequencePoint point)
        {
            TypeNode type = null;

            //local types not implemented

            //primitives
            if (parser.IsPrimitive(name))
                type = new TypeNode(parser, parser.GetPrimitive(name), scope, point);

            //imports
            if (type == null)
            {
                var types = globalImports.Select(namespaze => namespaze.GetContainedType(name)).Where(t => t != null);
                try
                {
                    if (types.Count() != 0)
                        type = new TypeNode(parser, types.Single(), scope, point);
                }
                catch (InvalidOperationException)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("Ambigious type {0}\n Could be:\n", name);
                    foreach (var t in types)
                    {
                        builder.Append(t.FullName);
                    }
                    throw new TypeException(point, builder.ToString());
                }
            }

            if (type == null)
            {
                if (parent != null)
                    type = parent.FindType(name, scope, point);
                else
                    parser.FindType(name, scope, point);
            }

            return type;
        }

        public NamespaceNode FindNamespace(string name, SequencePoint point)
        {
            NamespaceNode namespaze = null;

            var namespazes = globalImports.Select(import => import.GetContainedNamespace(name)).Where(n => n != null);
            try
            {
                if (namespazes.Count() != 0)
                    namespaze = new NamespaceNode(namespazes.Single(), point);
            }
            catch (InvalidOperationException)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("Ambigious namespace {0}\n Could be:\n", name);
                foreach (var n in namespazes)
                {
                    builder.Append(n);
                }
                throw new TypeException(point, builder.ToString());
            }

            if (namespaze == null)
            {
                if (parent != null)
                    namespaze = parent.FindNamespace(name, point);
                else
                    namespaze = parser.FindNamespace(name, point);
            }

            return namespaze;
        }

        public void AddImport(NamespaceNode namespaze, SequencePoint point)
        {
            if (globalImports.Any(n => n.Namespace == namespaze.Namespace.Namespace))
                throw new ParseException(point, "Namespace {0} already imported", namespaze);

            globalImports.Add(namespaze.Namespace);
        }

        #endregion type/namespace lookup

        #region parsing
        public void ParseDeclarations(AstNode lexerNode)
        {
            foreach (var node in lexerNode.Children)
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
                        throw new ParseException(parser.GetSequencePoint(node), "Import or declaration expected " + node.Type + " received");
                }
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
                var field = new InternalField(parser, this, declaration, parser.GetSequencePoint(lexerNode));
                fields.Add(field.Name, field);
            }
        }

        public void ParseInitializers()
        {
            foreach(var field in fields.Values)
            {
                field.Initialize(parser);
            }
        }

        public void Emit()
        {
            declaredMethods.ForEach(m => m.Emit());
        }

        #endregion parsing

        public void AddMethod(FunctionDeclarationNode method)
        {
            methods.Add(method.MethodReference.Name, method);
        }

        public string NewFunctionName()
        {
            return "$Lambda_" + lambdaCounter++.ToString();
        }

        public override string ToString()
        {
            string delim = "";
            StringBuilder builder = new StringBuilder("(ClassNode: Fields: ");
            foreach(var field in fields)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, field.Value.TypeWrapper.FullName, field.Key));
                if (field.Value.Initializer != null)
                    builder.Append(" = ").Append(field.Value.Initializer.ToString());
                delim = ", ";
            }

            builder.Append(" Methods: ");
            delim = "";
            foreach(var method in methods.Values)
            {
                builder.Append(method);
                delim = ", ";
            }
            
            return builder.Append(")").ToString();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Class:");
            builder.Indent(indent + 1).AppendLine("Fields:");
            foreach(var field in fields.Values)
            {
                builder.AppendLine(field.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Methods:");
            foreach(var method in methods.Values)
            {
                builder.AppendLine(method.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
