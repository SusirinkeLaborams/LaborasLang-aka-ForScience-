using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class AutoType : InternalType
    {
        public static TypeWrapper Instance { get; private set; }

        public override string FullName { get { return "auto"; } }

        private AutoType() : base(null, null)
        {
        }

        static AutoType()
        {
            Instance = new AutoType();
        }
    }
}
