using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    class Parser
    {
        public AssemblyEmitter Assembly { get; private set; }
        public ClassNode Root { get; set; }
        public string Filename { get; private set; }
        public Document Document { get; private set; }

        private bool testing;
        private Dictionary<string, TypeWrapper> primitives;

        #region typenames
        private const string tBool = "bool";
        private const string tChar = "char";
        private const string tByte = "byte";
        private const string tUByte = "ubyte";
        private const string tWord = "word";
        private const string tUWord = "uword";
        private const string tInt = "int";
        private const string tUInt = "uint";
        private const string tLong = "long";
        private const string tULong = "ulong";
        private const string tFloat = "float";
        private const string tDouble = "double";
        private const string tDecimal = "decimal";
        private const string tString = "string";
        private const string tVoid = "void";
        private const string tAuto = "auto";
        private const string tObject = "object";
        #endregion typenames

        #region types

        public TypeWrapper Bool { get; private set; }
        public TypeWrapper Char { get; private set; }
        public TypeWrapper Byte { get; private set; }
        public TypeWrapper UByte { get; private set; }
        public TypeWrapper UBool { get; private set; }
        public TypeWrapper Word { get; private set; }
        public TypeWrapper UWord { get; private set; }
        public TypeWrapper Int { get; private set; }
        public TypeWrapper UInt { get; private set; }
        public TypeWrapper Long { get; private set; }
        public TypeWrapper ULong { get; private set; }
        public TypeWrapper Float { get; private set; }
        public TypeWrapper Double { get; private set; }
        public TypeWrapper Decimal { get; private set; }
        public TypeWrapper String { get; private set; }
        public TypeWrapper Void { get; private set; }
        public TypeWrapper Auto { get; private set; }
        public TypeWrapper Object { get; private set; }

        #endregion types

        public Parser(AssemblyEmitter assembly, RootNode root, string filePath, bool testing = false)
        {
            Assembly = assembly;
            this.testing = testing;
            Filename = Path.GetFileNameWithoutExtension(filePath);
            Document = new Document(filePath);
            Document.Language = DocumentLanguage.Other;
            Document.LanguageVendor = DocumentLanguageVendor.Other;
            Document.Type = DocumentType.Text;
            this.primitives = new Dictionary<string, TypeWrapper>();

            primitives[tBool] = Bool = new ExternalType(assembly, typeof(bool));

            primitives[tChar] = Char = new ExternalType(assembly, typeof(char));
            primitives[tByte] = Byte = new ExternalType(assembly, typeof(sbyte));
            primitives[tUByte] = UByte = new ExternalType(assembly, typeof(byte));

            primitives[tWord] = Word = new ExternalType(assembly, typeof(short));
            primitives[tUWord] = UWord = new ExternalType(assembly, typeof(ushort));

            primitives[tInt] = Int = new ExternalType(assembly, typeof(int));
            primitives[tUInt] = UInt = new ExternalType(assembly, typeof(uint));

            primitives[tLong] = Long = new ExternalType(assembly, typeof(long));
            primitives[tULong] = ULong = new ExternalType(assembly, typeof(ulong));

            primitives[tFloat] = Float = new ExternalType(assembly, typeof(float));
            primitives[tDouble] = Double = new ExternalType(assembly, typeof(double));
            primitives[tDecimal] = Decimal = new ExternalType(assembly, typeof(decimal));

            primitives[tString] = String = new ExternalType(assembly, typeof(string));
            primitives[tObject] = Object = new ExternalType(assembly, typeof(object));

            primitives[tVoid] = Void = new ExternalType(assembly, typeof(void));
            primitives[tAuto] = Auto = null;

            var tree = root.Node;

            Root = new ClassNode(this, null, GetSequencePoint(tree));
            Root.ParseDeclarations(tree);
            Root.ParseBody(tree);
            if (!testing)
            {
                Root.DeclareMembers();
                Root.Emit();
            }
        }

        public SequencePoint GetSequencePoint(AstNode lexerNode)
        {
            var sequencePoint = new SequencePoint(Document);
            var start = lexerNode.Token.Start;
            var end = lexerNode.Token.End;
            sequencePoint.StartLine = start.Row;
            sequencePoint.StartColumn = start.Column;
            sequencePoint.EndLine = end.Row;
            sequencePoint.EndColumn = end.Column + 1;
            return sequencePoint; 
        }

        public SequencePoint GetSequencePoint(AstNode startNode, AstNode endNode)
        {
            var sequencePoint = new SequencePoint(Document);
            var start = startNode.Token.Start;
            var end = endNode.Token.End;
            sequencePoint.StartLine = start.Row;
            sequencePoint.StartColumn = start.Column;
            sequencePoint.EndLine = end.Row;
            sequencePoint.EndColumn = end.Column + 1;
            return sequencePoint;
        }

        public TypeNode FindType(string fullname, SequencePoint point)
        {
            var type = AssemblyRegistry.FindType(Assembly, fullname);
            if (type != null)
                return new TypeNode(new ExternalType(Assembly, type), point);
            else
                return null;
        }

        public NamespaceNode FindNamespace(string fullname, SequencePoint point)
        {
            if (AssemblyRegistry.IsNamespaceKnown(fullname))
                return new NamespaceNode(new ExternalNamespace(fullname, Assembly), point);
            else
                return null;
        }

        public bool IsPrimitive(string name)
        {
            return primitives.ContainsKey(name);
        }

        public TypeWrapper GetPrimitive(string name)
        {
            if (!IsPrimitive(name))
                return null;
            else
                return primitives[name];
        }
    }
}
