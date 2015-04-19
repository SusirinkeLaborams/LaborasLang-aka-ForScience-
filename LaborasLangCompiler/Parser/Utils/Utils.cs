using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Utils
{
    static class Utils
    {

        public static StringBuilder Indent(this StringBuilder builder, int count)
        {
            builder.Append('\t', count);
            return builder;
        }

        public static IReadOnlyList<T> Enumerate<T>(this T item)
        {
            return new T[] { item };
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }

        public static IEnumerable<T> Enumerate<T>(params T[] stuff)
        {
            return stuff;
        }

        public static IEnumerable<T> ConcatAll<T>(IEnumerable<IEnumerable<T>> enumerables)
        {
            var result = enumerables.First();
            foreach(var e in enumerables.Skip(1))
            {
                result = result.Concat(e);
            }
            return result;
        }

        public static T Fail<T>()
        {
            throw new Exception();
        }
    }
}
