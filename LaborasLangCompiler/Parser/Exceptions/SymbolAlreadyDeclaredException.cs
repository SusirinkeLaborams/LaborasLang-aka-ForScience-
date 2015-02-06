using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    [Obsolete]
    class SymbolAlreadyDeclaredException : CompilerException
    {
        public SymbolAlreadyDeclaredException(SequencePoint point, string message) : base(point, message) { }
        public SymbolAlreadyDeclaredException(SequencePoint point, string format, params object[] message) : base(point, format, message) { }
    }
}
