using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    [ContractClass(typeof(IAbstractSyntaxTreeContract))]
    public interface IAbstractSyntaxTree
    {
        IReadOnlyList<IAbstractSyntaxTree> Children { get; }
        TokenType Type { get; }
        Node Node { get; }
        string Content { get; }
        string FullContent { get; }
    }

    [ContractClassFor(typeof(IAbstractSyntaxTree))]
    public abstract class IAbstractSyntaxTreeContract : IAbstractSyntaxTree
    {
        public IReadOnlyList<IAbstractSyntaxTree> Children
        {
            get 
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<IAbstractSyntaxTree>>() != null);
                throw new NotImplementedException();
            }
        }

        public TokenType Type
        {
            get { throw new NotImplementedException(); }
        }

        public Node Node
        {
            get
            {
                Contract.Ensures(Contract.Result<Node>() != null);
                throw new NotImplementedException();
            }
        }

        public string Content
        {
            get { throw new NotImplementedException(); }
        }

        public string FullContent
        {
            get { throw new NotImplementedException(); }
        }
    }
}
