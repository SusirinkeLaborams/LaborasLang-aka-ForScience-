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
        public bool ShouldEmit { get; private set; }

        private Dictionary<string, TypeWrapper> primitives;

        #region types

        public TypeWrapper Bool { get; private set; }
        public TypeWrapper Char { get; private set; }
        public TypeWrapper Int8 { get; private set; }
        public TypeWrapper UInt8 { get; private set; }
        public TypeWrapper Int16 { get; private set; }
        public TypeWrapper UInt16 { get; private set; }
        public TypeWrapper Int32 { get; private set; }
        public TypeWrapper UInt32 { get; private set; }
        public TypeWrapper Int64 { get; private set; }
        public TypeWrapper UInt64 { get; private set; }
        public TypeWrapper Float { get; private set; }
        public TypeWrapper Double { get; private set; }
        public TypeWrapper Decimal { get; private set; }
        public TypeWrapper String { get; private set; }
        public TypeWrapper Void { get; private set; }
        public TypeWrapper Auto { get; private set; }
        public TypeWrapper Object { get; private set; }

        #endregion types

        private Parser(AssemblyEmitter assembly, RootNode root, string filePath, bool emit = true)
        {
            Assembly = assembly;
            Filename = Path.GetFileNameWithoutExtension(filePath);
            Document = new Document(filePath);
            Document.Language = DocumentLanguage.Other;
            Document.LanguageVendor = DocumentLanguageVendor.Other;
            Document.Type = DocumentType.Text;
            ShouldEmit = emit;
            this.primitives = new Dictionary<string, TypeWrapper>();

            primitives["bool"] = Bool = new ExternalType(assembly, typeof(bool));

            primitives["char"] = Char = new ExternalType(assembly, typeof(char));
            primitives["int8"] = Int8 = new ExternalType(assembly, typeof(sbyte));
            primitives["uint8"] = UInt8 = new ExternalType(assembly, typeof(byte));

            primitives["int16"] = Int16 = new ExternalType(assembly, typeof(short));
            primitives["uint16"] = UInt16 = new ExternalType(assembly, typeof(ushort));

            primitives["int32"] = primitives["int"] = Int32 = new ExternalType(assembly, typeof(int));
            primitives["uint32"] = primitives["uint"] = UInt32 = new ExternalType(assembly, typeof(uint));

            primitives["int64"] = primitives["long"] = Int64 = new ExternalType(assembly, typeof(long));
            primitives["uint64"] = primitives["ulong"] = UInt64 = new ExternalType(assembly, typeof(ulong));

            primitives["float"] = Float = new ExternalType(assembly, typeof(float));
            primitives["double"] = Double = new ExternalType(assembly, typeof(double));
            primitives["decimal"] = Decimal = new ExternalType(assembly, typeof(decimal));

            primitives["string"] = String = new ExternalType(assembly, typeof(string));
            primitives["object"] = Object = new ExternalType(assembly, typeof(object));

            primitives["void"] = Void = new ExternalType(assembly, typeof(void));
            primitives["auto"] = Auto = AutoType.Instance;

            var tree = root.Node;

            Root = new ClassNode(this, null, tree);
        }

        public static Parser ParseAll(AssemblyEmitter assembly, RootNode root, string filePath, bool emit = true)
        {
            Parser parser = new Parser(assembly, root, filePath, emit);
            parser.Root.ParseDeclarations();
            parser.Root.ParseInitializers();
            parser.Root.Emit();
            return parser;
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

        public SequencePoint GetSequencePoint(AstNode start, AstNode end)
        {
            var sequencePoint = new SequencePoint(Document);
            sequencePoint.StartLine = start.Token.Start.Row;
            sequencePoint.StartColumn = start.Token.Start.Column;
            sequencePoint.EndLine = end.Token.Start.Row;
            sequencePoint.EndColumn = end.Token.Start.Column + 1;
            return sequencePoint;
        }

        public SequencePoint GetSequencePoint(SequencePoint start, SequencePoint end)
        {
            var sequencePoint = new SequencePoint(start.Document);
            sequencePoint.StartLine = start.StartLine;
            sequencePoint.StartColumn = start.StartColumn;
            sequencePoint.EndLine = end.EndLine;
            sequencePoint.EndColumn = end.EndColumn;
            return sequencePoint;
        }

        public TypeWrapper FindType(string fullname)
        {
            var type = AssemblyRegistry.FindType(Assembly, fullname);
            if (type != null)
                return new ExternalType(Assembly, type);
            else
                return null;
        }

        public NamespaceWrapper FindNamespace(string fullname)
        {
            if (AssemblyRegistry.IsNamespaceKnown(fullname))
                return new ExternalNamespace(fullname, Assembly);
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
