namespace Cali.Syntax
{
    public interface IIdentifiableSyntax : IStatementSyntax
    {
        string Identifier { get; }
    }
}