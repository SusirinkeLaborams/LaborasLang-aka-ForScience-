using NPEG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.LexingTools
{
    static class FileReader
    {
        public static ByteInputIterator Read(string filename)
        {
            var bytes = System.IO.File.ReadAllBytes(filename);
            return new ByteInputIterator(bytes);
        }
    }
}
