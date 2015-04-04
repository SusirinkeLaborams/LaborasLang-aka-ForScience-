using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    abstract class PostProcessor
    {

        public static IEnumerable<PostProcessor> BuildAll(RootNode root)
        {
            return new PostProcessor[]{new ArrayFunctionResolver(root)};
        }
        public unsafe RootNode RootNode { get; set; }

        public PostProcessor(RootNode root)
        {
            this.RootNode = root;
        }
        public void Transform(AstNode tree)
        {
            Traverse(tree);
        }

        private void Traverse(AstNode tree)
        {
 	        for(int i = 0; i < tree.ChildrenCount; i++)
            {
                Transform(RootNode, tree.Children[i]);
                Traverse(tree.Children[i]);
            }
        }

        public abstract void Transform(RootNode RootNode, AstNode astNode);
    }
}
