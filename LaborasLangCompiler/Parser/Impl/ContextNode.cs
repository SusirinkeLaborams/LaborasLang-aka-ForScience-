using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ContextNode : ParserNode
    {
        public ContextNode Parent { get; private set; }
        public Parser Parser { get; private set; }
        public AssemblyEmitter Assembly { get { return Parser.Assembly; } }

        public abstract FunctionDeclarationNode GetMethod();
        public abstract ClassNode GetClass();
        public abstract bool IsStaticContext();
        public abstract ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point);

        protected ContextNode(Parser parser, ContextNode parent, SequencePoint point) : base(point)
        {
            Parser = parser;
            Parent = parent;
        }
    }
}
