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

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ParserNode, IContainerNode
    {
        public override NodeType Type { get { return NodeType.ClassNode; } }
        private Dictionary<string, FieldDeclarationNode> fields;
        private List<string> globalImports;
        private Dictionary<string, string> namedImports;
        private ClassNode parent;
        private Parser parser;
        public TypeEmitter TypeEmitter { get; private set; }
        private List<Tuple<string, FunctionDeclarationNode>> methods = new List<Tuple<string,FunctionDeclarationNode>>();
        private int lambdaCounter = 0;
        private ClassNode(Parser parser, ClassNode parent, SequencePoint point) : base(point)
        {
            this.parent = parent;
            this.parser = parser;
            fields = new Dictionary<string, FieldDeclarationNode>();
            globalImports = new List<string>();
            globalImports.Add("");
            namedImports = new Dictionary<string, string>();
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        private void AddField(string name, TypeReference type, SequencePoint point)
        {
            fields.Add(name, new FieldDeclarationNode(name, type, point));
        }
        public void AddMethod(FunctionDeclarationNode method, string name)
        {
            methods.Add(Tuple.Create(name, method));
        }
        private FieldNode GetField(string name)
        {
            if (fields.ContainsKey(name))
                return fields[name];

            if (parent != null)
                return parent.GetField(name);

            return null;
        }
        public ClassNode GetClass() { return this; }
        public FunctionDeclarationNode GetFunction() { return null; }
        public LValueNode GetSymbol(string name)
        {
            return GetField(name);
        }
        public TypeNode FindType(string name, SequencePoint point)
        {
            if (parser.Primitives.ContainsKey(name))
                return new TypeNode(parser.Primitives[name], point);

            var types = globalImports.Select(namespaze => AssemblyRegistry.GetType(parser.Assembly, namespaze + name)).Where(t => t != null);
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
                throw new TypeException(builder.ToString());
            }

            if(type != null)
                return new TypeNode(type, point);

            return null;
        }
        public TypeNode FindType(TypeNode main, string nested, SequencePoint point)
        {
            var type = AssemblyRegistry.GetType(parser.Assembly, main.ParsedType.FullName + "." + nested);
            if(type != null)
                return new TypeNode(type, point);

            return null;
        }
        public TypeNode FindType(NamespaceNode namespaze, string name, SequencePoint point)
        {
            var type = AssemblyRegistry.GetType(parser.Assembly, namespaze.Value + "." + name);
            if (type != null)
                return new TypeNode(type, point);

            return null;
        }
        public NamespaceNode FindNamespace(string name, SequencePoint point)
        {
            string namespaze = null;
            if (namedImports.ContainsKey(name))
                namespaze = namedImports[name];
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
                throw new TypeException(builder.ToString());
            }

            return new NamespaceNode(namespaze, point);
        }
        public void AddImport(string namespaze, string name = null)
        {
            if(name == null)
            {
                if (globalImports.Contains(namespaze))
                    throw new ParseException(String.Format("Namespace {0} already imported", namespaze));
                globalImports.Add(namespaze + ".");
            }
            else
            {
                if (namedImports.ContainsKey(name))
                    throw new ParseException(String.Format("Namespace under name {0} already imported", name));
                if (namedImports.ContainsValue(namespaze))
                    throw new ParseException(String.Format("Namespace {0} already imported", namespaze));
                namedImports.Add(name, namespaze + ".");
            }
        }
        public NamespaceNode FindNamespace(NamespaceNode left, string right, SequencePoint point)
        {
            var full = left.Value + "." + right;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new NamespaceNode(full, point);

            return null;
        }
        private void AddFieldToEmitter(Parser parser, FieldDefinition field, IExpressionNode init)
        {
            if (!parser.Testing)
                TypeEmitter.AddField(field, init);
        }
        public static ClassNode Parse(Parser parser, ClassNode parentClass, AstNode lexerNode)
        {
            var instance = new ClassNode(parser, parentClass, parser.GetSequencePoint(lexerNode));
            AstNode sentence;

            if (parser.Root == null)
            {
                parser.Root = instance;
            }

            //symbols
            foreach (var node in lexerNode.Children)
            {
                if (node.Token.Name == Lexer.Sentence)
                {
                    sentence = node.Children[0];
                    switch (sentence.Token.Name)
                    {
                        case Lexer.NamespaceImport:
                            ImportNode.Parse(parser, instance, sentence);
                            break;
                        case Lexer.Declaration:
                            ParseDeclaration(parser, instance, sentence, false);
                            break;
                        case Lexer.DeclarationAndAssignment:
                            ParseDeclaration(parser, instance, sentence, true);
                            break;
                        default:
                            throw new ParseException("Import or declaration expected " + sentence.Token.Name + " received");
                    }
                }
                else
                {
                    throw new ParseException("Node Sentence expected, " + node.Token.Name + " received");
                }
            }

            //init
            foreach (var node in lexerNode.Children)
            {
                sentence = node.Children[0];
                switch (sentence.Token.Name)
                {
                    case Lexer.DeclarationAndAssignment:
                        IExpressionNode init = null;
                        var field = instance.fields[parser.ValueOf(sentence.Children[1])];
                        if (sentence.Children[2].Token.Name == Lexer.Function)
                            init = FunctionDeclarationNode.Parse(parser, instance, sentence.Children[2], field.Name);
                        else
                            init = ExpressionNode.Parse(parser, instance, sentence.Children[2]);
                        field.Initializer = init;
                        if (field.ReturnType == null)
                        {
                            field.ReturnType = init.ReturnType;
                        }
                        else
                        {
                            if (!ILHelpers.IsAssignableTo(init.ReturnType, field.ReturnType))
                                throw new TypeException("Type mismatch, field " + field.Name + " type " + field.ReturnType.FullName + " initialized with " + init.ReturnType.FullName);
                        }
                        break;
                    default:
                        break;
                }
            }

            //field declarations
            foreach(var node in lexerNode.Children)
            {
                sentence = node.Children[0];
                switch (sentence.Token.Name)
                {
                    case Lexer.Declaration:
                    case Lexer.DeclarationAndAssignment:
                        var field = instance.fields[parser.ValueOf(sentence.Children[1])];
                        field.CreateFieldDefinition(FieldAttributes.Static | FieldAttributes.Private);
                        if(field.Initializer is FunctionDeclarationNode)
                        {
                            instance.AddMethod((FunctionDeclarationNode)field.Initializer, field.Name);
                        }
                        instance.AddFieldToEmitter(parser, (FieldDefinition)field.Field, field.Initializer);
                        break;
                    default:
                        break;
                }
            }
            if (!parser.Testing)
            {
                foreach (var method in instance.methods)
                {
                    method.Item2.Emit(method.Item1 == "Main");
                }
            }
            return instance;
        }
        private static void ParseDeclaration(Parser parser, ClassNode klass, AstNode lexerNode, bool init)
        {
            var declaredType = TypeNode.Parse(parser, klass, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            
            if (declaredType == null && !init)
            {
                throw new TypeException("Type inference requires initialization");
            }
            else
            {
                //worst special case ever
                //jei deklaruojam funkcija be tipo, jos tipas isparsinamas
                //tipas reikalingas rekursijai
                if (declaredType == null && lexerNode.Children[2].Token.Name == Lexer.Function)
                {
                    declaredType = FunctionDeclarationNode.ParseType(parser, klass, lexerNode.Children[2]);
                    klass.AddField(name, declaredType, parser.GetSequencePoint(lexerNode));
                }
                else
                {
                    klass.AddField(name, declaredType, parser.GetSequencePoint(lexerNode));
                }
            }
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
