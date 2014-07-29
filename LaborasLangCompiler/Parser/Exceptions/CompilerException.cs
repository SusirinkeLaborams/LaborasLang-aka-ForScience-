using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    class CompilerException : System.Exception
    {
        public CompilerException(SequencePoint point, string message) : base(message + Environment.NewLine + point.ToString()){}
        public CompilerException(SequencePoint point, string format, params object[] message) : base(String.Format(format, message) + Environment.NewLine + point.ToString()) { }
    }
}
