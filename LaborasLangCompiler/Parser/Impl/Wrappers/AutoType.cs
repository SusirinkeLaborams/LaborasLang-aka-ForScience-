using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class AutoType : TypeReference
    {
        public static TypeReference Instance { get; private set; }

        public override string FullName { get { return "auto"; } }

        private AutoType() : base("", "auto")
        {
        }

        static AutoType()
        {
            Instance = new AutoType();
        }
    }
}
