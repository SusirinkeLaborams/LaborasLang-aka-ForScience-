using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    [Serializable()]
    class SymbolNotFoundException : System.Exception
    {
        public SymbolNotFoundException() : base() { }
        public SymbolNotFoundException(string message) : base(message) { }
        public SymbolNotFoundException(string message, System.Exception inner) : base(message, inner) { }

        protected SymbolNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}