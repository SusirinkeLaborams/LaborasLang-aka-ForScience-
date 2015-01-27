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

        private Dictionary<string, TypeReference> primitives;

        #region types

        public TypeReference Bool { get; private set; }
        public TypeReference Char { get; private set; }
        public TypeReference Int8 { get; private set; }
        public TypeReference UInt8 { get; private set; }
        public TypeReference Int16 { get; private set; }
        public TypeReference UInt16 { get; private set; }
        public TypeReference Int32 { get; private set; }
        public TypeReference UInt32 { get; private set; }
        public TypeReference Int64 { get; private set; }
        public TypeReference UInt64 { get; private set; }
        public TypeReference Float { get; private set; }
        public TypeReference Double { get; private set; }
        public TypeReference Decimal { get; private set; }
        public TypeReference String { get; private set; }
        public TypeReference Void { get; private set; }
        public TypeReference Auto { get; private set; }
        public TypeReference Object { get; private set; }

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
            this.primitives = new Dictionary<string, TypeReference>();

            primitives["bool"] = Bool = assembly.TypeToTypeReference(typeof(bool));

            primitives["char"] = Char = assembly.TypeToTypeReference(typeof(char));
            primitives["int8"] = Int8 = assembly.TypeToTypeReference(typeof(sbyte));
            primitives["uint8"] = UInt8 = assembly.TypeToTypeReference(typeof(byte));

            primitives["int16"] = Int16 = assembly.TypeToTypeReference(typeof(short));
            primitives["uint16"] = UInt16 = assembly.TypeToTypeReference(typeof(ushort));

            primitives["int32"] = primitives["int"] = Int32 = assembly.TypeToTypeReference(typeof(int));
            primitives["uint32"] = primitives["uint"] = UInt32 = assembly.TypeToTypeReference(typeof(uint));

            primitives["int64"] = primitives["long"] = Int64 = assembly.TypeToTypeReference(typeof(long));
            primitives["uint64"] = primitives["ulong"] = UInt64 = assembly.TypeToTypeReference(typeof(ulong));

            primitives["float"] = Float = assembly.TypeToTypeReference(typeof(float));
            primitives["double"] = Double = assembly.TypeToTypeReference(typeof(double));
            primitives["decimal"] = Decimal = assembly.TypeToTypeReference(typeof(decimal));

            primitives["string"] = String = assembly.TypeToTypeReference(typeof(string));
            primitives["object"] = Object = assembly.TypeToTypeReference(typeof(object));

            primitives["void"] = Void = assembly.TypeToTypeReference(typeof(void));
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

        public TypeReference FindType(string fullname)
        {
            return AssemblyRegistry.FindType(Assembly, fullname);
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

        public TypeReference GetPrimitive(string name)
        {
            if (!IsPrimitive(name))
                return null;
            else
                return primitives[name];
        }
    }
}
