using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ParserNode : IParserNode
    {
        public SequencePoint SequencePoint { get; private set; }
        public abstract NodeType Type { get; }
        protected ParserNode(SequencePoint sequencePoint)
        {
            SequencePoint = sequencePoint;
        }
    }
}
