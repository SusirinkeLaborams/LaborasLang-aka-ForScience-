using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lexer.PostProcessors
{
    abstract class BottomUpPostProcessor : PostProcessor
    {
        public override void Apply(AbstractSyntaxTree tree)
        {
            tree.Children.ForEach(t =>
            {
                Apply(t);
                Transform(t);   
            });
        }
    }
}
