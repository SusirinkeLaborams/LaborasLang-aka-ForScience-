using LaborasLangCompiler.ILTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Tree
{
    abstract class ParserNodeImplementation : ParserNode
    {
    }

    abstract class Function : ParserNode
    {
        public abstract MethodEmitter Method { get; }

        public abstract List<FunctionArgumentNode> Arguments { get; }

        public abstract List<ParserNode> Body { get; }
    }
}
