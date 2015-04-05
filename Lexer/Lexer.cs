using Lexer.Containers;
using Lexer.PostProcessors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Lexer
{
    public sealed class Lexer
    {
        public static IAbstractSyntaxTree Lex(string source)
        {
            using (var root = new RootNode())
            {

                var sourceTokens = Tokenizer.Tokenize(source, root);
                var syntaxMatcher = new SyntaxMatcher(sourceTokens, root);

                var nodes = syntaxMatcher.Match();
                var exposedTree = new AbstractSyntaxTree(nodes);
                foreach (var postProcessor in PostProcessor.BuildAll())
                {
                    postProcessor.Apply(exposedTree);
                }
                nodes.Cleanup(root);
                return exposedTree;
            }

        }

    }
}
