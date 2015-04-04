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
        [Obsolete]
        public static RootNode Lex(string source)
        {
            var rootNode = new RootNode();
            AstNodeExtractor.Invoke(rootNode, source);
          
            return rootNode;
        }

        public static void WithTree(string source, Action<AstNode> consumer)
        {
            using (var rootNode = new RootNode())
            {
                consumer.Invoke(AstNodeExtractor.Invoke(rootNode, source));
            }
        }

        public static void WithTree(IReadOnlyList<string> sources, Action<IReadOnlyList<AstNode>> consumer)
        {
            var roots = new List<RootNode>();
            try
            {
                roots.AddRange(sources.Select(source => new RootNode()));

                var nodes = roots.Zip(sources, AstNodeExtractor).ToArray();
                consumer.Invoke(nodes);
            }
            finally
            {
                foreach (var rootNode in roots)
                {
                    rootNode.Dispose();
                }
            }
        }

        private static Func<RootNode, string, AstNode> AstNodeExtractor
        {
            get
            {
                return (root, source) =>
                {
                    var sourceTokens = Tokenizer.Tokenize(source, root);
                    var syntaxMatcher = new SyntaxMatcher(sourceTokens, root);

                    var nodes = syntaxMatcher.Match();

                    foreach (var postProcessor in PostProcessor.BuildAll(root))
                    {
                        postProcessor.Transform(nodes);
                    }

                    return nodes;
                };
            }
        }
    }
}
