using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    [Serializable()]
    class SymbolAlreadyDeclaredException : System.Exception
    {
        public SymbolAlreadyDeclaredException() : base() { }
        public SymbolAlreadyDeclaredException(string message) : base(message) { }
        public SymbolAlreadyDeclaredException(string message, System.Exception inner) : base(message, inner) { }

        protected SymbolAlreadyDeclaredException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
