using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    class TypeException : CompilerException
    {
        public TypeException(SequencePoint point, string message) : base(point, message) { }
        public TypeException(SequencePoint point, string format, params object[] message) : base(point, format, message) { }
    }
}
