using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Parser.Tree;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    class Parser
    {
        public AssemblyRegistry Registry { get; private set; }
        public IParserNode Root { get; set; }

        public Parser(AssemblyRegistry registry, AstNode tree)
        {
            Registry = registry;
            ParserNode.Parse(this, null, tree);
        }
    }
}
