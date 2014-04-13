using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.Utilities
{
    class SourceReader
    {
        public static ByteInputIterator ReadSource(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return new ByteInputIterator(bytes);
        }
    }
}
