using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl;
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
        public IReadOnlyDictionary<string, TypeReference> Primitives { get; private set; }
        public bool Testing { get; private set; }
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
            Testing = testing;
            this.source = source;
            Filename = Path.GetFileNameWithoutExtension(filePath);
            Document = new Document(filePath);
            Document.Language = DocumentLanguage.Other;
            Document.LanguageVendor = DocumentLanguageVendor.Other;
            Document.Type = DocumentType.Text;
            SymbolCounter = new SymbolCounter(source);

            var primitives = new Dictionary<string, TypeReference>();
            primitives.Add(Bool, Assembly.TypeToTypeReference(typeof(bool)));

            primitives.Add(Char, Assembly.TypeToTypeReference(typeof(char)));
            primitives.Add(Byte, Assembly.TypeToTypeReference(typeof(sbyte)));
            primitives.Add(UByte, Assembly.TypeToTypeReference(typeof(byte)));

            primitives.Add(Word, Assembly.TypeToTypeReference(typeof(short)));
            primitives.Add(UWord, Assembly.TypeToTypeReference(typeof(ushort)));

            primitives.Add(Int, Assembly.TypeToTypeReference(typeof(int)));
            primitives.Add(UInt, Assembly.TypeToTypeReference(typeof(uint)));

            primitives.Add(Long, Assembly.TypeToTypeReference(typeof(long)));
            primitives.Add(ULong, Assembly.TypeToTypeReference(typeof(ulong)));

            primitives.Add(Float, Assembly.TypeToTypeReference(typeof(float)));
            primitives.Add(Double, Assembly.TypeToTypeReference(typeof(double)));
            primitives.Add(Decimal, Assembly.TypeToTypeReference(typeof(decimal)));

            primitives.Add(String, Assembly.TypeToTypeReference(typeof(string)));
            primitives.Add(Object, Assembly.TypeToTypeReference(typeof(object)));

            primitives.Add(Void, Assembly.TypeToTypeReference(typeof(void)));
            primitives.Add(Auto, null);

            Primitives = primitives;

            ClassNode.Parse(this, null, tree);
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
    }
}
