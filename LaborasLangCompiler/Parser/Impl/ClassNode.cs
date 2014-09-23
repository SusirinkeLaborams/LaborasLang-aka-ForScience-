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
    class ClassNode : ParserNode, ContainerNode
    {
        #region fields
        private Dictionary<string, InternalField> fields;
        private List<NamespaceWrapper> globalImports;
        private ClassNode parent;
        private Parser parser;
        private Dictionary<string, FunctionDeclarationNode> methods = new Dictionary<string, FunctionDeclarationNode>();
        private int lambdaCounter = 0;
        #endregion fields

        #region properties
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public TypeEmitter TypeEmitter { get; private set; }
        public string FullName { get; private set; }
        public TypeReference TypeReference { get { return TypeEmitter.Get(parser.Assembly); } }
        #endregion properties

        public ClassNode(Parser parser, ClassNode parent, SequencePoint point) : base(point)
        {
            if (parser.Root == null)
                parser.Root = this;
            this.parent = parent;
            this.parser = parser;
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
            var ret = new List<MethodWrapper>(1);
            if (methods.ContainsKey(name))
                ret.Add(methods[name]);
            return ret;
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

        #region container

        public ClassNode GetClass() 
        { 
            return this;
        
        }

        public FunctionDeclarationNode GetFunction() 
        {
            return null;
        }

        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            var field = GetField(name);
            if (field != null)
                return new FieldNode(null, field, point);

            if (parent != null)
                return parent.GetSymbol(name, point);

            return null;
        }

        #endregion container

        #region type/namespace lookup

        public TypeNode FindType(string name, SequencePoint point)
        {
            TypeNode type = null;

            //local types not implemented

            //primitives
            if (parser.IsPrimitive(name))
                type = new TypeNode(parser.GetPrimitive(name), point);

            //imports
            if (type == null)
            {
                var types = globalImports.Select(namespaze => namespaze.GetContainedType(name)).Where(t => t != null);
                try
                {
                    if (types.Count() != 0)
                        type = new TypeNode(types.Single(), point);
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
                    type = parent.FindType(name, point);
                else
                    parser.FindType(name, point);
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
            var field = new InternalField(declaration);

            field.Name = declaration.SymbolName.GetSingleSymbolOrThrow();
            field.TypeWrapper = TypeNode.Parse(parser, this, declaration.Type);

            if (field.TypeWrapper == null && !declaration.Initializer.IsNull && declaration.Initializer.IsFunctionDeclaration())
                field.TypeWrapper = FunctionDeclarationNode.ParseFunctorType(parser, this, declaration.Initializer.Children[0]);

            fields.Add(field.Name, field);
        }

        public void ParseBody(AstNode lexerNode)
        {
            foreach(var field in fields.Values)
            {
                var init = field.Declaration.Initializer;
                if (!init.IsNull)
                {
                    field.Initializer = ExpressionNode.Parse(parser, this, init);

                    if(field.TypeWrapper == null)
                    {
                        field.TypeWrapper = field.Initializer.TypeWrapper;
                    }
                    else
                    {
                        if (!field.Initializer.TypeWrapper.IsAssignableTo(field.TypeWrapper))
                            throw new TypeException(field.Initializer.SequencePoint, "Type mismatch, field " + field.Name + " type " + field.TypeWrapper.FullName + " initialized with " + field.Initializer.TypeWrapper.FullName);
                    }
                }
            }
        }

        public void DeclareMembers()
        {
            foreach (var f in fields)
            {
                var field = f.Value;
                field.FieldDefinition = new FieldDefinition(field.Name, FieldAttributes.Private | FieldAttributes.Static, field.TypeWrapper.TypeReference);
                AddFieldToEmitter(field);
            }
        }

        public void Emit()
        {
            foreach (var m in methods)
            {
                m.Value.Emit(m.Key == "$Main");
            }
        }

        #endregion parsing

        public void AddMethod(FunctionDeclarationNode method, string name)
        {
            methods.Add(name, method);
        }

        private void AddFieldToEmitter(InternalField field)
        {
            TypeEmitter.AddField(field.FieldDefinition, field.Initializer);
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
    }
}
