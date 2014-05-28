using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPEG;

namespace LaborasLangCompiler.LexingTools
{
    class SymbolCounter
    {
        private static Dictionary<string, FilePosition[]> m_Positions = new Dictionary<string, FilePosition[]>();

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

        public static void Load(string file)
        {
            var text = File.ReadAllText(file);
            var bytes = Encoding.UTF8.GetBytes(text);
            
            int row = 1, column = 1;
            FilePosition[] positions = new FilePosition[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                positions[i] = new FilePosition(row, column);

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

            m_Positions.Add(file, positions);
        }

        public static void Unload(string file)
        {
            m_Positions.Remove(file);
        }

        public static FilePosition Get(string file, int symbol)
        {
            FilePosition[] positions;
            if (m_Positions.TryGetValue(file, out positions))
            {
                return positions[symbol];
            }
            else
            {
                return new FilePosition(0, 0);
            }
        }
    }
}
