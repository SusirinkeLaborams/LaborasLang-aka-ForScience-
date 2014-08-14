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
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        private Dictionary<string, InternalField> fields;
        private List<string> globalImports;
        private ClassNode parent;
        private Parser parser;
        public TypeEmitter TypeEmitter { get; private set; }
        public string FullName { get; private set; }
        public TypeReference TypeReference { get { return TypeEmitter.Get(parser.Assembly); } }
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
            FullName = parser.Filename;
            TypeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        public void AddMethod(FunctionDeclarationNode method, string name)
        {
            methods.Add(name, method);
        }
        public FieldWrapper GetField(string name)
        {
            if (fields.ContainsKey(name))
                return fields[name];

            return null;
        }
        public MethodWrapper GetMethod(string name)
        {
            if (methods.ContainsKey(name))
                return methods[name];
            else
                return null;
        }
        public IEnumerable<MethodWrapper> GetMethods(string name)
        {
            var ret = new List<MethodWrapper>(1);
            if (methods.ContainsKey(name))
                ret.Add(methods[name]);
            return ret;
        }
        public bool IsAssignableTo(TypeWrapper other)
        {
            return FullName == other.FullName;
        }
        public TypeWrapper GetContainedType(string name) { return null; }
        public ClassNode GetClass() { return this; }
        public FunctionDeclarationNode GetFunction() { return null; }
        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            var field = GetField(name);
            if (field != null)
                return new FieldNode(null, field, point);

            if (parent != null)
                return parent.GetSymbol(name, point);

            return null;
        }
        public TypeNode FindType(string name, SequencePoint point)
        {
            if (parser.Primitives.ContainsKey(name))
                return new TypeNode(parser.Primitives[name], point);
            TypeWrapper type = null;
            var types = globalImports.Select(namespaze => parser.FindType(namespaze + name)).Where(t => t != null);
            if (types.Count() == 0)
                return null;
            try
            {
                type = types.Single();
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

            return new TypeNode(type, point);
        }
        public NamespaceNode FindNamespace(string name, SequencePoint point)
        {
            NamespaceWrapper namespaze = null;
            var namespazes = globalImports.Select(import => parser.FindNamespace(import + name)).Where(n => n != null);
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
        private void AddFieldToEmitter(InternalField field)
        {
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
                        var field = fields[parser.ValueOf(sentence.Children[1])];
                        var init = sentence.Children[2];

                        if (sentence.Children[2].Token.Name == Lexer.Function)
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
                field.FieldDefinition = new FieldDefinition(field.Name, FieldAttributes.Private | FieldAttributes.Static, field.TypeWrapper.TypeReference);
                AddFieldToEmitter(field);
            }
        }
        public void Emit()
        {
            foreach(var m in methods)
            {
                m.Value.Emit(m.Key == "$Main");
            }
        }
        private void ParseDeclaration(AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, this, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            //nera tipo, deklaruojam funkcija
            if(lexerNode.Children.Count > 2 && lexerNode.Children[2].Token.Name == Lexer.Function && type == null)
            {
                type = FunctionDeclarationNode.ParseFunctorType(parser, this, lexerNode.Children[2]);
            }
            fields.Add(name, new InternalField(type, name));
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
