using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Common
{
    class NullType : TypeReference
    {
        public static TypeReference Instance { get; private set; }

        public override string FullName { get { return "null"; } }

        private NullType()
            : base("", "null")
        {
        }

        static NullType()
        {
            Instance = new NullType();
        }
    }
}
