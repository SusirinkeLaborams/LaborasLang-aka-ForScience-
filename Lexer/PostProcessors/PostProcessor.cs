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

        public static IEnumerable<PostProcessor> BuildAll()
        {
            return new PostProcessor[]{new ArrayFunctionResolver()};
        }

        public PostProcessor()
        {
        }

        private void Traverse(AbstractSyntaxTree tree)
        {
            tree.Children.ForEach(t =>
            {
                Transform(t);
                Traverse(t);
            });
        }

        public abstract void Transform(AbstractSyntaxTree astNode);

    }
}
