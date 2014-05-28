using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPEG;

namespace LaborasLangCompiler.LexingTools
{
    class SymbolCounter
    {
        public FilePosition[] Positions { get; private set; }

        public struct FilePosition
        {
            public int row;
            public int column;

            public FilePosition(int row, int column)
            {
                this.row = row;
                this.column = column;
            }
        };

        public SymbolCounter(ByteInputIterator file)
        {
            Positions = new FilePosition[file.Length];
            int row = 1, column = 1;
            var bytes = file.Text(0, file.Length - 1);
            for (int i = 0; i < bytes.Length; i++)
            {
                Positions[i] = new FilePosition(row, column);

                if (Encoding.UTF8.GetString(bytes, i, 1) == "\n")
                {
                    row++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
        }
    }
}
