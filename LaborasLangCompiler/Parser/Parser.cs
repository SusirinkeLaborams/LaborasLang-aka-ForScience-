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

        public Parser(AssemblyEmitter assembly, RootNode root, string filePath, bool emit = true)
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
            primitives["auto"] = Auto = null;

            var tree = root.Node;

            Root = new ClassNode(this, null, GetSequencePoint(tree));
            Root.ParseDeclarations(tree);
            Root.ParseInitializers();
            Root.Emit();
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
