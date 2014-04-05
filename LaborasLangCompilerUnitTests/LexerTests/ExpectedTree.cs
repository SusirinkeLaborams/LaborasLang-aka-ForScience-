using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    class AstHelper
    {
        public static string Stringify(AstNode tree)
        {
            if (tree.Children == null || tree.Children.Count == 0)
            {
                return tree.Token.Name;
            }
            else if (tree.Children.Count == 1)
            {
                return string.Format("{0}: {1}", tree.Token.Name, Stringify(tree.Children[0]));
            }
            else
            {
                StringBuilder childrenString = new StringBuilder();
                var first = true;
                foreach(var child in tree.Children)
                {                
                    if(first)
                    { 
                        first = false;
                    }
                    else
                    {
                        childrenString.Append(", ");
                    }
                    childrenString.Append(Stringify(child));
                }
                return string.Format("{0}: ({1})", tree.Token.Name, childrenString.ToString());
            }
        }
    }
}
