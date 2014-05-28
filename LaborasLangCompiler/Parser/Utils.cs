using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    static class Utils
    {
        public static bool IsSettable(this IExpressionNode node)
        {
            if (node.ExpressionType != ExpressionNodeType.LValue)
                return false;
            return true;
            //properties vistiek dar neveikia
        }
        public static bool IsGettable(this IExpressionNode node)
        {
            //kol kas viskas gettable
            return true;
        }
    }
}
