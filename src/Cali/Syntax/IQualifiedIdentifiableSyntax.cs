namespace Cali.Syntax
{
    public interface IQualifiedIdentifiableSyntax : IIdentifiableSyntax
    {
        string Namespace { get; }
    }
}