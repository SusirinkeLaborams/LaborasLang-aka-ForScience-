using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Misc
{
    internal static class Utils
    {
        public static string CombinePaths(params string[] paths)
        {
            string resultingPath = string.Empty;

            foreach (var path in paths)
            {
                resultingPath = Path.Combine(resultingPath, path);
            }

            return resultingPath;
        }
    }
}