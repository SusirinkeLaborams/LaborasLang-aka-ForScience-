using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
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
        public SymbolCounter SymbolCounter { get; private set; }
        private ByteInputIterator source;
        public IReadOnlyDictionary<string, TypeWrapper> Primitives { get; private set; }
        private bool testing;
        public const string Bool   = "bool";
        public const string Char   = "char";
        public const string Byte   = "byte";
        public const string UByte  = "ubyte";
        public const string Word   = "word";
        public const string UWord  = "uword";
        public const string Int    = "int";
        public const string UInt   = "uint";
        public const string Long   = "long";
        public const string ULong  = "ulong";
        public const string Float  = "float";
        public const string Double = "double";
        public const string Decimal = "decimal";
        public const string String = "string";
        public const string Void   = "void";
        public const string Auto   = "auto";
        public const string Object = "object";
        public Parser(AssemblyEmitter assembly, AstNode tree, ByteInputIterator source, string filePath, bool testing = false)
        {
            Assembly = assembly;
            this.testing = testing;
            this.source = source;
            Filename = Path.GetFileNameWithoutExtension(filePath);
            Document = new Document(filePath);
            Document.Language = DocumentLanguage.Other;
            Document.LanguageVendor = DocumentLanguageVendor.Other;
            Document.Type = DocumentType.Text;
            SymbolCounter = new SymbolCounter(source);

            var primitives = new Dictionary<string, TypeWrapper>();
            primitives.Add(Bool, new ExternalType(assembly, typeof(bool)));

            primitives.Add(Char, new ExternalType(assembly, typeof(char)));
            primitives.Add(Byte, new ExternalType(assembly, typeof(sbyte)));
            primitives.Add(UByte, new ExternalType(assembly, typeof(byte)));

            primitives.Add(Word, new ExternalType(assembly, typeof(short)));
            primitives.Add(UWord, new ExternalType(assembly, typeof(ushort)));

            primitives.Add(Int, new ExternalType(assembly, typeof(int)));
            primitives.Add(UInt, new ExternalType(assembly, typeof(uint)));

            primitives.Add(Long, new ExternalType(assembly, typeof(long)));
            primitives.Add(ULong, new ExternalType(assembly, typeof(ulong)));

            primitives.Add(Float, new ExternalType(assembly, typeof(float)));
            primitives.Add(Double, new ExternalType(assembly, typeof(double)));
            primitives.Add(Decimal, new ExternalType(assembly, typeof(decimal)));

            primitives.Add(String, new ExternalType(assembly, typeof(string)));
            primitives.Add(Object, new ExternalType(assembly, typeof(object)));

            primitives.Add(Void, new ExternalType(assembly, typeof(void)));
            primitives.Add(Auto, null);

            Primitives = primitives;

            Root = new ClassNode(this, null, GetSequencePoint(tree));
            Root.ParseDeclarations(tree);
            Root.ParseBody(tree);
            if (!testing)
            {
                Root.DeclareMembers();
                Root.Emit();
            }
        }
        public string ValueOf(AstNode node)
        {
            return Encoding.UTF8.GetString(source.Text(node.Token.Start, node.Token.End));
        }
        public SequencePoint GetSequencePoint(AstNode lexerNode)
        {
            var sequencePoint = new SequencePoint(Document);
            var start = SymbolCounter.Positions[lexerNode.Token.Start];
            var end = SymbolCounter.Positions[lexerNode.Token.End];
            sequencePoint.StartLine = start.row;
            sequencePoint.StartColumn = start.column;
            sequencePoint.EndLine = end.row;
            sequencePoint.EndColumn = end.column + 1;
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
    }
}
