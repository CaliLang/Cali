namespace Cali.Syntax
{
    public interface ITypedSyntax : IStatementSyntax
    {
        TypeReferenceSyntax Type { get; }
    }
}