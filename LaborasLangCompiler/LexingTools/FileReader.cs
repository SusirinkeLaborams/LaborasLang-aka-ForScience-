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
            // First 2 bytes of a text file is its encoding.. 
            // read text instead and then convert to bytes
            var text = File.ReadAllText(filename);
            var bytes = Encoding.UTF8.GetBytes(text);
            return new ByteInputIterator(bytes);
        }
    }
}
