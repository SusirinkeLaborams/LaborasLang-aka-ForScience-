using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ProjectParser : IDisposable
    {
        public AssemblyEmitter Assembly { get; private set; }
        public bool ShouldEmit { get; private set; }

        private Dictionary<string, TypeReference> primitives;
        private List<Parser> parsers;

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

        private ProjectParser(AssemblyEmitter assembly, bool emit)
        {
            Assembly = assembly;
            ShouldEmit = emit;
            this.primitives = new Dictionary<string, TypeReference>();
            this.parsers = new List<Parser>();

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
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, string[] sources, string[] names, bool emit)
        {
            
            ProjectParser projectParser = new ProjectParser(assembly, emit);
            for(int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                var name = names[i];
                projectParser.parsers.Add(new Parser(projectParser, Lexer.Lexer.Lex(source), name));
            }

            projectParser.parsers.ForEach(p => p.Root.ParseDeclarations());
            projectParser.parsers.ForEach(p => p.Root.ParseInitializers());
            projectParser.parsers.ForEach(p => p.Root.Emit());
            return projectParser;
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, string[] files, bool emit)
        {
            return ParseAll(assembly, files.Select(f => File.ReadAllText(f)).ToArray(), files, emit);
        }

        public TypeReference FindType(string fullname)
        {
            return AssemblyRegistry.FindType(Assembly, fullname);
        }

        public Namespace FindNamespace(string fullname)
        {
            if (AssemblyRegistry.IsNamespaceKnown(fullname))
                return new Namespace(fullname, Assembly);
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

        public void Dispose()
        {
            foreach(var parser in parsers)
            {
                parser.Dispose();
            }
        }

        public override string ToString()
        {
            return string.Join("\r\n", parsers.Select(p => p.Root.ToString(0)));
        }
    }
}
