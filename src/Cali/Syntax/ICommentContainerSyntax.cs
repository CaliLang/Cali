using System.Collections.Generic;

namespace Cali.Syntax
{
    public interface ICommentContainerSyntax : IStatementSyntax
    {
        ICollection<CommentSyntax> Comments { get; }
    }
}