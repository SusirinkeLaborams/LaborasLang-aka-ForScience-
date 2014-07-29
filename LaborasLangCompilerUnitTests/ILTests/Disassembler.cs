using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests
{
    static class Disassembler
    {
        public static string DisassembleAssembly(string path)
        {
            var module = ModuleDefinition.ReadModule(path);
            return DisassembleAssembly(module);
        }

        public static string DisassembleAssembly(ModuleDefinition module)
        {
            StringWriter writer;
            var disassembler = CreateDisassembler(out writer);

            foreach (var type in module.Types)
            {
                DisassembleType(type, disassembler, writer);

                foreach (var nestedType in type.NestedTypes)
                {
                    DisassembleType(nestedType, disassembler, writer);
                }
            }

            return writer.ToString();
        }

        public static string DisassembleType(TypeDefinition type)
        {
            StringWriter writer;
            var disassembler = CreateDisassembler(out writer);

            DisassembleType(type, disassembler, writer);

            foreach (var nestedType in type.NestedTypes)
            {
                DisassembleType(nestedType, disassembler, writer);
            }

            return writer.ToString();
        }

        public static string DisassembleMethod(MethodDefinition method)
        {
            if (!method.HasBody)
            {
                throw new ArgumentException("Method has no body!", "method");
            }

            StringWriter writer;
            var disassembler = CreateDisassembler(out writer);

            writer.WriteLine(method.FullName.ToString());
            disassembler.Disassemble(method.Body, null);
            writer.WriteLine();
            writer.WriteLine();

            return writer.ToString();
        }

        private static void DisassembleType(TypeDefinition type, MethodBodyDisassembler disassembler, StringWriter writer)
        {
            foreach (var method in type.Methods.Where(method => method.HasBody))
            {
                writer.WriteLine(method.FullName.ToString());
                disassembler.Disassemble(method.Body, null);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        private static MethodBodyDisassembler CreateDisassembler(out StringWriter writer)
        {
            writer = new StringWriter();
            var disassembler = new MethodBodyDisassembler(new PlainTextOutput(writer), true, new CancellationToken());

            return disassembler;
        }
    }
}
