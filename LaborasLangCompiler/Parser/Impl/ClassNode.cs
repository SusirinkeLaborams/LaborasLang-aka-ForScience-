using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.LexingTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ParserNode, IContainerNode
    {
        public override NodeType Type { get { return NodeType.ClassNode; } }
        private Dictionary<string, InternalField> fields;
        private List<string> globalImports;
        private ClassNode parent;
        private Parser parser;
        public TypeEmitter TypeEmitter { get; private set; }
        private Dictionary<string, FunctionDeclarationNode> methods = new Dictionary<string, FunctionDeclarationNode>();
        private int lambdaCounter = 0;
        public ClassNode(Parser parser, ClassNode parent, SequencePoint point) : base(point)
        {
            if (parser.Root == null)
                parser.Root = this;
            this.parent = parent;
            this.parser = parser;
            fields = new Dictionary<string, InternalField>();
            globalImports = new List<string>();
            globalImports.Add("");
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        private void AddField(string name, TypeReference type)
        {
            fields.Add(name, new InternalField(type, name));
        }
        public void AddMethod(FunctionDeclarationNode method, string name)
        {
            methods.Add(name, method);
        }
        private FieldNode GetField(string name, SequencePoint point)
        {
            if (fields.ContainsKey(name))
                return new FieldNode(null, fields[name], point);

            if (parent != null)
                return parent.GetField(name, point);

            return null;
        }
        public ClassNode GetClass() { return this; }
        public FunctionDeclarationNode GetFunction() { return null; }
        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            return GetField(name, point);
        }
        public TypeNode FindType(string name, SequencePoint point)
        {
            if (parser.Primitives.ContainsKey(name))
                return new TypeNode(parser.Primitives[name], point);

            var types = globalImports.Select(namespaze => AssemblyRegistry.FindType(parser.Assembly, namespaze + name)).Where(t => t != null);
            if(types.Count() == 0)
                return null;
            TypeReference type = null;
            try
            {
                type = types.Single();
            }
            catch(InvalidOperationException)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("Ambigious type {0}\n Could be:\n", name);
                foreach(var t in types)
                {
                    builder.Append(t.FullName);
                }
                throw new TypeException(point, builder.ToString());
            }

            if(type != null)
                return new TypeNode(type, point);

            return null;
        }
        public TypeNode FindType(TypeNode main, string nested, SequencePoint point)
        {
            var type = AssemblyRegistry.FindType(parser.Assembly, main.ParsedType.FullName + "." + nested);
            if(type != null)
                return new TypeNode(type, point);

            return null;
        }
        public TypeNode FindType(NamespaceNode namespaze, string name, SequencePoint point)
        {
            var type = AssemblyRegistry.FindType(parser.Assembly, namespaze.Value + "." + name);
            if (type != null)
                return new TypeNode(type, point);

            return null;
        }
        public NamespaceNode FindNamespace(string name, SequencePoint point)
        {
            string namespaze = null;
            var namespazes = globalImports.Select(import => AssemblyRegistry.IsNamespaceKnown(import + name) ? import + name : null).Where(n => n != null);
            if (namespazes.Count() == 0)
                return null;
            try
            {
                namespaze = namespazes.Single();
            }
            catch(InvalidOperationException)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("Ambigious namespace {0}\n Could be:\n", name);
                foreach (var n in namespazes)
                {
                    builder.Append(n);
                }
                throw new TypeException(point, builder.ToString());
            }

            return new NamespaceNode(namespaze, point);
        }
        public void AddImport(string namespaze, SequencePoint point)
        {
            if (globalImports.Contains(namespaze))
                throw new ParseException(point, "Namespace {0} already imported", namespaze);
            globalImports.Add(namespaze + ".");
        }
        public NamespaceNode FindNamespace(NamespaceNode left, string right, SequencePoint point)
        {
            var full = left.Value + "." + right;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new NamespaceNode(full, point);

            return null;
        }
        private void AddFieldToEmitter(InternalField field)
        {
            if (!parser.Testing)
                TypeEmitter.AddField(field.FieldDefinition, field.Initializer);
        }
        public void ParseDeclarations(AstNode lexerNode)
        {
            foreach (var node in lexerNode.Children)
            {
                if (node.Token.Name == Lexer.Sentence)
                {
                    AstNode sentence = node.Children[0];
                    switch (sentence.Token.Name)
                    {
                        case Lexer.NamespaceImport:
                            ImportNode.Parse(parser, this, sentence);
                            break;
                        case Lexer.Declaration:
                        case Lexer.DeclarationAndAssignment:
                            ParseDeclaration(sentence);
                            break;
                        default:
                            throw new ParseException(parser.GetSequencePoint(sentence), "Import or declaration expected " + sentence.Token.Name + " received");
                    }
                }
                else
                {
                    throw new ParseException(parser.GetSequencePoint(node), "Node Sentence expected, " + node.Token.Name + " received");
                }
            }
        }
        public void ParseBody(AstNode lexerNode)
        {
            foreach (var node in lexerNode.Children)
            {
                AstNode sentence = node.Children[0];
                switch (sentence.Token.Name)
                {
                    case Lexer.DeclarationAndAssignment:
                        IExpressionNode init = null;
                        var field = fields[parser.ValueOf(sentence.Children[1])];
                        if (sentence.Children[2].Token.Name == Lexer.Function)
                            init = FunctionDeclarationNode.Parse(parser, this, sentence.Children[2], field.Name);
                        else
                            init = ExpressionNode.Parse(parser, this, sentence.Children[2]);
                        field.Initializer = init;
                        if (field.ReturnType == null)
                        {
                            field.ReturnType = init.ReturnType;
                        }
                        else
                        {
                            if (!ILHelpers.IsAssignableTo(init.ReturnType, field.ReturnType))
                                throw new TypeException(parser.GetSequencePoint(sentence), "Type mismatch, field " + field.Name + " type " + field.ReturnType.FullName + " initialized with " + init.ReturnType.FullName);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public void DeclareMembers()
        {
            foreach(var f in fields)
            {
                var field = f.Value;
                field.FieldDefinition = new FieldDefinition(field.Name, FieldAttributes.Private | FieldAttributes.Static, field.ReturnType);
                AddFieldToEmitter(field);
            }
            foreach(var m in methods.Values)
            {
                //later
            }
        }
        public void Emit()
        {
            foreach(var m in methods)
            {
                m.Value.Emit(m.Key == "Main");
            }
        }
        private void ParseDeclaration(AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, this, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            //nera tipo, deklaruojam funkcija
            if(type == null && lexerNode.Children.Count > 2 && lexerNode.Children[2].Token.Name == Lexer.Function)
            {
                type = FunctionDeclarationNode.ParseType(parser, this, lexerNode.Children[2]);
            }
            AddField(name, type);
        }
        public string NewFunctionName()
        {
            return "Lambda_" + lambdaCounter++.ToString();
        }
        public override string ToString()
        {
            string delim = "";
            StringBuilder builder = new StringBuilder("(ClassNode: Fields: ");
            foreach(var field in fields)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, field.Value.ReturnType.FullName, field.Key));
                if (field.Value.Initializer != null)
                    builder.Append(" = ").Append(field.Value.Initializer.ToString());
                delim = ", ";
            }
            
            return builder.Append(")").ToString();
        }
    }
}
