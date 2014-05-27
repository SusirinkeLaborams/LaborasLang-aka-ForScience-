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

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ParserNode, IContainerNode
    {
        public override NodeType Type { get { return NodeType.ClassNode; } }
        private Dictionary<string, FieldDeclarationNode> fields;
        private Dictionary<string, TypeReference> declaredTypes;
        private ClassNode parent;
        private Parser parser;
        public TypeEmitter TypeEmitter { get; private set; }
        private List<Tuple<string, FunctionDeclarationNode>> methods = new List<Tuple<string,FunctionDeclarationNode>>();
        private int lambdaCounter = 0;
        private ClassNode(Parser parser, ClassNode parent)
        {
            this.parent = parent;
            this.parser = parser;
            fields = new Dictionary<string, FieldDeclarationNode>();
            declaredTypes = new Dictionary<string, TypeReference>();
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        private void AddField(string name, TypeReference type)
        {
            fields.Add(name, new FieldDeclarationNode(name, type));
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
        public TypeNode FindType(string name)
        {
            if (declaredTypes.ContainsKey(name))
                return new TypeNode(declaredTypes[name]);

            var type = AssemblyRegistry.GetType(parser.Assembly, name);
            if(type != null)
                return new TypeNode(type);

            return null;
        }
        public TypeNode FindType(TypeNode main, string nested)
        {
            var type = AssemblyRegistry.GetType(parser.Assembly, main.ParsedType.FullName + "." + nested);
            if(type != null)
                return new TypeNode(type);

            return null;
        }
        public TypeNode FindType(NamespaceNode namespaze, string name)
        {
            var type = AssemblyRegistry.GetType(parser.Assembly, namespaze.Value + "." + name);
            if (type != null)
                return new TypeNode(type);

            return null;
        }
        public NamespaceNode FindNamespace(string name)
        {
            if (AssemblyRegistry.IsNamespaceKnown(name))
                return new NamespaceNode(name);

            return null;
        }
        public NamespaceNode FindNamespace(NamespaceNode left, string right)
        {
            var full = left.Value + "." + right;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new NamespaceNode(full);

            return null;
        }
        private void AddFieldToEmitter(Parser parser, FieldDefinition field, IExpressionNode init)
        {
            if (!parser.Testing)
                TypeEmitter.AddField(field, init);
        }
        public static ClassNode Parse(Parser parser, ClassNode parentClass, AstNode lexerNode)
        {
            var instance = new ClassNode(parser, parentClass);
            AstNode sentence;

            if (parser.Root == null)
            {
                parser.Root = instance;
                instance.AddDefaultAliases();
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
                            throw new NotImplementedException();
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
            var declaredType = parser.ParseType(lexerNode.Children[0]);
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
                    klass.AddField(name, declaredType);
                }
                else
                {
                    klass.AddField(name, declaredType);
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
        private void AddDefaultAliases()
        {
            declaredTypes.Add(Parser.Bool, parser.Assembly.TypeToTypeReference(typeof(bool)));

            declaredTypes.Add(Parser.Char, parser.Assembly.TypeToTypeReference(typeof(char)));
            declaredTypes.Add(Parser.Byte, parser.Assembly.TypeToTypeReference(typeof(sbyte)));
            declaredTypes.Add(Parser.UByte, parser.Assembly.TypeToTypeReference(typeof(byte)));

            declaredTypes.Add(Parser.Word, parser.Assembly.TypeToTypeReference(typeof(short)));
            declaredTypes.Add(Parser.UWord, parser.Assembly.TypeToTypeReference(typeof(ushort)));

            declaredTypes.Add(Parser.Int, parser.Assembly.TypeToTypeReference(typeof(int)));
            declaredTypes.Add(Parser.UInt, parser.Assembly.TypeToTypeReference(typeof(uint)));

            declaredTypes.Add(Parser.Long, parser.Assembly.TypeToTypeReference(typeof(long)));
            declaredTypes.Add(Parser.ULong, parser.Assembly.TypeToTypeReference(typeof(ulong)));

            declaredTypes.Add(Parser.Float, parser.Assembly.TypeToTypeReference(typeof(float)));
            declaredTypes.Add(Parser.Double, parser.Assembly.TypeToTypeReference(typeof(double)));
            declaredTypes.Add(Parser.Decimal, parser.Assembly.TypeToTypeReference(typeof(decimal)));

            declaredTypes.Add(Parser.String, parser.Assembly.TypeToTypeReference(typeof(string)));

            declaredTypes.Add(Parser.Void, parser.Assembly.TypeToTypeReference(typeof(void)));
            declaredTypes.Add(Parser.Object, parser.Assembly.TypeToTypeReference(typeof(object)));
            declaredTypes.Add(Parser.Auto, null);
        }
    }
}
