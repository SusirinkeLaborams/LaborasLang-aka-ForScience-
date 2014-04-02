using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;

namespace LexerUnitTests
{
    class ExpectedTree
    {
        public List<ExpectedTree> Children;
        public String Token;

        public ExpectedTree(String token, params string[] children)
        {
            Token = token;
            Children = new List<ExpectedTree>();
            foreach(var child in children)
            {
                Children.Add(new ExpectedTree(child));
            }
        }

        public ExpectedTree(string token, params ExpectedTree[] children)
        {
            Token = token;
            Children = new List<ExpectedTree>();
            foreach (var child in children)
            {
                Children.Add(child);
            }
        }

        public ExpectedTree(string token)
        {
            Token = token;
            Children = null;
        }

        public void AsertEqual(AstNode actualTree)
        {
            Assert.AreEqual(Token, actualTree.Token.Name);
            if (Children != null)
            {
                Assert.AreEqual(Children.Count, actualTree.Children.Count);
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].AsertEqual(actualTree.Children[i]);
                }
            }
        }
    }
}
