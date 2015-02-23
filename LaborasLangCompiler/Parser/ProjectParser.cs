using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Emitters;
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
        public AssemblyEmitter Assembly { get { return Emitters.Assembly; } }
        public IEmitterSource Emitters { get; private set; }

        private readonly Dictionary<string, TypeReference> aliases;
        private readonly HashSet<TypeReference> primitives;

        public IReadOnlyDictionary<ulong, TypeReference> MaxValues { get; private set; }
        public IReadOnlyDictionary<long, TypeReference> MinValues { get; private set; }

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
        public TypeReference Decimal { get; private set; }
        public TypeReference String { get; private set; }
        public TypeReference Void { get; private set; }
        public TypeReference Auto { get; private set; }
        public TypeReference Object { get; private set; }

        #endregion types

        private ProjectParser(IEmitterSource emitters)
        {
            Emitters = emitters;
            this.aliases = new Dictionary<string, TypeReference>();
            this.parsers = new List<Parser>();

            aliases["bool"] = Bool = Assembly.TypeSystem.Boolean;

            aliases["int8"] = Int8 = Assembly.TypeSystem.SByte;
            aliases["uint8"] = UInt8 = Assembly.TypeSystem.Byte;

            aliases["int16"] = Int16 = Assembly.TypeSystem.Int16;
            aliases["uint16"] = UInt16 = Assembly.TypeSystem.UInt16;
            aliases["char"] = Char = Assembly.TypeSystem.Char;

            aliases["int32"] = aliases["int"] = Int32 = Assembly.TypeSystem.Int32;
            aliases["uint32"] = aliases["uint"] = UInt32 = Assembly.TypeSystem.UInt32;

            aliases["int64"] = aliases["long"] = Int64 = Assembly.TypeSystem.Int64;
            aliases["uint64"] = aliases["ulong"] = UInt64 = Assembly.TypeSystem.UInt64;

            aliases["float"] = Float = Assembly.TypeSystem.Single;
            aliases["double"] = Double = Assembly.TypeSystem.Double;
            aliases["decimal"] = Decimal = AssemblyRegistry.FindType(Assembly, "System.Decimal");

            aliases["string"] = String = Assembly.TypeSystem.String;
            aliases["object"] = Object = Assembly.TypeSystem.Object;

            aliases["void"] = Void = Assembly.TypeSystem.Void;
            aliases["auto"] = Auto = AutoType.Instance;

            primitives = new HashSet<TypeReference>(Utils.Utils.Enumerate(Bool, Char, Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float, Double, Decimal, String), new TypeComparer());

            MaxValues = new Dictionary<ulong, TypeReference>()
            {
                {(ulong)sbyte.MaxValue, Int8},
                {byte.MaxValue, UInt8},
                {(ulong)short.MaxValue, Int16},
                {ushort.MaxValue, UInt16},
                {int.MaxValue, Int32},
                {uint.MaxValue, UInt32},
                {long.MaxValue, Int64},
                {ulong.MaxValue, UInt64}
            };

            MinValues = new Dictionary<long, TypeReference>()
            {
                {sbyte.MinValue, Int8},
                {short.MinValue, Int16},
                {int.MinValue, Int32},
                {long.MinValue, Int64},
            };
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

        public static ProjectParser ParseAll(IEmitterSource emitters, string[] sources, string[] names)
        {
            
            ProjectParser projectParser = new ProjectParser(emitters);
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

        public static ProjectParser ParseAll(IEmitterSource emitters, string[] files)
        {
            return ParseAll(emitters, files.Select(f => File.ReadAllText(f)).ToArray(), files);
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
