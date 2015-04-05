using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer;
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
        public ProjectParser ProjectParser { get; private set; }
        public AssemblyEmitter Assembly { get { return ProjectParser.Assembly; } }
        public ClassNode Root { get; set; }
        public string Filename { get; private set; }
        public Document Document { get; private set; }

        public TypeReference Bool { get { return ProjectParser.Bool; } }
        public TypeReference Char { get { return ProjectParser.Char; } }
        public TypeReference Int8 { get { return ProjectParser.Int8; } }
        public TypeReference UInt8 { get { return ProjectParser.UInt8; } }
        public TypeReference Int16 { get { return ProjectParser.Int16; } }
        public TypeReference UInt16 { get { return ProjectParser.UInt16; } }
        public TypeReference Int32 { get { return ProjectParser.Int32; } }
        public TypeReference UInt32 { get { return ProjectParser.UInt32; } }
        public TypeReference Int64 { get { return ProjectParser.Int64; } }
        public TypeReference UInt64 { get { return ProjectParser.UInt64; } }
        public TypeReference Float { get { return ProjectParser.Float; } }
        public TypeReference Double { get { return ProjectParser.Double; } }
        public TypeReference String { get { return ProjectParser.String; } }
        public TypeReference Void { get { return ProjectParser.Void; } }
        public TypeReference Auto { get { return ProjectParser.Auto; } }
        public TypeReference Object { get { return ProjectParser.Object; } }

        public Parser(ProjectParser parser, string filePath)
        {
            Filename = Path.GetFileNameWithoutExtension(filePath);
            Document = new Document(filePath);
            Document.Language = DocumentLanguage.Other;
            Document.LanguageVendor = DocumentLanguageVendor.Other;
            Document.Type = DocumentType.Text;
            ProjectParser = parser;

            Root = ClassNode.ForFile(this);
        }

        public void ParseDeclarations(AbstractSyntaxTree root)
        {
            Root.ParseDeclarations(root);
        }

        public void ParseInitializers()
        {
            Root.ParseInitializers();
        }

        public void Emit()
        {
            Root.Emit();
        }

        public SequencePoint GetSequencePoint(AbstractSyntaxTree lexerNode)
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

        public SequencePoint GetSequencePoint(AbstractSyntaxTree start, AbstractSyntaxTree end)
        {
            var sequencePoint = new SequencePoint(Document);
            sequencePoint.StartLine = start.Token.Start.Row;
            sequencePoint.StartColumn = start.Token.Start.Column;
            sequencePoint.EndLine = end.Token.Start.Row;
            sequencePoint.EndColumn = end.Token.Start.Column + 1;
            return sequencePoint;
        }

        public static SequencePoint GetSequencePoint(SequencePoint start, SequencePoint end)
        {
            if (start == null)
                return end;
            if (end == null)
                return start;

            var sequencePoint = new SequencePoint(start.Document);
            sequencePoint.StartLine = start.StartLine;
            sequencePoint.StartColumn = start.StartColumn;
            sequencePoint.EndLine = end.EndLine;
            sequencePoint.EndColumn = end.EndColumn;
            return sequencePoint;
        }

        public bool IsPrimitive(TypeReference type)
        {
            return ProjectParser.IsPrimitive(type);
        }
    }
}
