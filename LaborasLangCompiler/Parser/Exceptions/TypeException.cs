using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Exceptions
{
    [Serializable()]
    class TypeException : System.Exception
    {
        public TypeException() : base() { }
        public TypeException(string message) : base(message) { }
        public TypeException(string message, System.Exception inner) : base(message, inner) { }

        protected TypeException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
