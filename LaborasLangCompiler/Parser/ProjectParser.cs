using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer;
using Lexer.Containers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ProjectParser
    {
        public AssemblyEmitter Assembly { get; private set; }
        public bool ShouldEmit { get; private set; }

        private readonly Dictionary<string, TypeReference> aliases;
        private readonly HashSet<TypeReference> primitives;

        private readonly List<Parser> parsers;

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
        public TypeReference String { get; private set; }
        public TypeReference Void { get; private set; }
        public TypeReference Auto { get; private set; }
        public TypeReference Object { get; private set; }

        #endregion types

        private ProjectParser(AssemblyEmitter assembly, bool emit)
        {
            Assembly = assembly;
            ShouldEmit = emit;
            this.aliases = new Dictionary<string, TypeReference>();
            this.parsers = new List<Parser>();

            aliases["bool"] = Bool = assembly.TypeSystem.Boolean;

            aliases["int8"] = Int8 = assembly.TypeSystem.SByte;
            aliases["uint8"] = UInt8 = assembly.TypeSystem.Byte;

            aliases["int16"] = Int16 = assembly.TypeSystem.Int16;
            aliases["uint16"] = UInt16 = assembly.TypeSystem.UInt16;
            aliases["char"] = Char = assembly.TypeSystem.Char;

            aliases["int32"] = aliases["int"] = Int32 = assembly.TypeSystem.Int32;
            aliases["uint32"] = aliases["uint"] = UInt32 = assembly.TypeSystem.UInt32;

            aliases["int64"] = aliases["long"] = Int64 = assembly.TypeSystem.Int64;
            aliases["uint64"] = aliases["ulong"] = UInt64 = assembly.TypeSystem.UInt64;

            aliases["float"] = Float = assembly.TypeSystem.Single;
            aliases["double"] = Double = assembly.TypeSystem.Double;

            aliases["string"] = String = assembly.TypeSystem.String;
            aliases["object"] = Object = assembly.TypeSystem.Object;

            aliases["void"] = Void = assembly.TypeSystem.Void;
            aliases["auto"] = Auto = AutoType.Instance;

            primitives = new HashSet<TypeReference>(Utils.Utils.Enumerate(Bool, Char, Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float, Double, String), new TypeComparer());

            
        }

        private class TypeComparer : IEqualityComparer<TypeReference>
        {
            public bool Equals(TypeReference x, TypeReference y)
            {
                return x.FullName.Equals(y.FullName);
            }
            public int GetHashCode(TypeReference obj)
            {
                return obj.FullName.GetHashCode();
            }
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, CompilerArguments compilerArguments, string[] sources, bool emit)
        {
            Contract.Requires(compilerArguments.SourceFiles.Count == sources.Length);

            ProjectParser projectParser = new ProjectParser(assembly, emit);
            var roots = sources.Select(s => Lexer.Lexer.Lex(s)).ToArray();

            for (int i = 0; i < sources.Length; i++)
            {
                var filePath = compilerArguments.SourceFiles[i];
                projectParser.parsers.Add(new Parser(projectParser, filePath, compilerArguments.FileToNamespaceMap[filePath]));
            }

            for (int i = 0; i < sources.Length; i++)
            {
                projectParser.parsers[i].ParseDeclarations(roots[i]);
            }
            projectParser.parsers.ForEach(p => p.ParseInitializers());
            projectParser.parsers.ForEach(p => p.Emit());
            
            return projectParser;
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, CompilerArguments compilerArguments, bool emit)
        {
            return ParseAll(assembly, compilerArguments, compilerArguments.SourceFiles.Select(f => File.ReadAllText(f)).ToArray(), emit);
        }

        public TypeReference FindType(string fullname)
        {
            if (aliases.ContainsKey(fullname))
                return aliases[fullname];
            else
                return AssemblyRegistry.FindType(Assembly, fullname);
        }

        public Namespace FindNamespace(string fullname)
        {
            if (fullname.Length > 0 && AssemblyRegistry.IsNamespaceKnown(fullname))
                return new Namespace(fullname, Assembly);
            else
                return null;
        }

        public bool IsPrimitive(TypeReference type)
        {
            return primitives.Contains(type);
        }

        public override string ToString()
        {
            return string.Join("\r\n", parsers.Select(p => p.Root.ToString(0)));
        }
    }
}
