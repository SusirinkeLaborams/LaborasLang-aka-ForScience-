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

        public void AddImport(string namespaze, SequencePoint point)
        {
            if (globalImports.Any(n => n.Namespace == namespaze))
                throw new ParseException(point, "Namespace {0} already imported", namespaze);

            var found = FindNamespace(namespaze, point);
            if (found != null)
                globalImports.Add(found.Namespace);
            else
                throw new ParseException(point, "Unknown namespace {0}", namespaze);
        }

        #endregion type/namespace lookup

        #region parsing
        public void ParseDeclarations(AstNode lexerNode)
        {
            foreach (var node in lexerNode.Children)
            {
                if (node.Type == Lexer.TokenType.StatementNode)
                {
                    AstNode sentence = node.Children[0];
                    switch (sentence.Type)
                    {
                        case Lexer.TokenType.Namespace:
                            ImportNode.Parse(parser, this, sentence);
                            break;
                        case Lexer.TokenType.DeclarationNode:
                            ParseDeclaration(sentence);
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(sentence), "Import or declaration expected " + sentence.Type + " received");
                    }
                }
                else
                {
                    throw new ParseException(parser.GetSequencePoint(node), "Node Sentence expected, " + node.Type + " received");
                }
            }
        }

        private void ParseDeclaration(AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, this, lexerNode.Children[0]);
            var name = lexerNode.Children[1].Content.ToString();
            //nera tipo, deklaruojam funkcija
            if (lexerNode.Children.Count > 2 && lexerNode.Children[2].Type == Lexer.TokenType.Function && type == null)
            {
                type = FunctionDeclarationNode.ParseFunctorType(parser, this, lexerNode.Children[2]);
            }
            fields.Add(name, new InternalField(type, name));
        }

        public void ParseBody(AstNode lexerNode)
        {
            foreach (var node in lexerNode.Children)
            {
                AstNode sentence = node.Children[0];
                //temp code
                if(sentence.Type == Lexer.TokenType.DeclarationNode && sentence.Children.Count > 2)
                {
                    var field = fields[sentence.Children[1].Content.ToString()];
                    var init = sentence.Children[2];

                    if (sentence.Children[2].Type == Lexer.TokenType.Function)
                    {
                        field.Initializer = MethodNode.Parse(parser, this, init, "$" + field.Name);
                    }
                    else
                    {
                        field.Initializer = ExpressionNode.Parse(parser, this, init);
                    }
                    if (field.TypeWrapper == null)
                    {
                        field.TypeWrapper = field.Initializer.TypeWrapper;
                    }
                    else
                    {
                        if (!field.Initializer.TypeWrapper.IsAssignableTo(field.TypeWrapper))
                            throw new TypeException(parser.GetSequencePoint(sentence), "Type mismatch, field " + field.Name + " type " + field.TypeWrapper.FullName + " initialized with " + field.Initializer.TypeWrapper.FullName);
                    }
                }
                switch (sentence.Type)
                {
                    case Lexer.TokenType.DeclarationNode:
                        
                        break;
                    default:
                        break;
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
