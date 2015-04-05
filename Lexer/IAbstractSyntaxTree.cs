using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public interface IAbstractSyntaxTree
    {
        IReadOnlyList<IAbstractSyntaxTree> Children { get; }
        TokenType Type { get; }
        Node Node { get; }
        string Content { get; }
        string FullContent { get; }
    }
}
