using LaborasLangCompiler.Parser.Tree;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ParserNode : IParserNode
    {
        public abstract NodeType Type { get; }
        public CodeBlockNode ParentBlock { get; private set; }
        public virtual ILValueNode GetSymbol(string name)
        {
            if (ParentBlock != null)
                return ParentBlock.GetSymbol(name);
            else
                return null;
        }
        public ParserNode(CodeBlockNode parent)
        {
            ParentBlock = parent;
        }

        public static ParserNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            throw new NotImplementedException();
        }
    }
}
