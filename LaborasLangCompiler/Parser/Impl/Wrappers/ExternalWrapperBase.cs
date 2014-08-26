using LaborasLangCompiler.ILTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalWrapperBase
    {
        protected AssemblyEmitter Assembly { get; private set; }
        protected ExternalWrapperBase(AssemblyEmitter assembly)
        {
            this.Assembly = assembly;
        }
    }
}
