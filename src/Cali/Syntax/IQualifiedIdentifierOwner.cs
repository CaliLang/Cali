namespace Cali.Syntax
{
    public interface IQualifiedIdentifierOwner : IIdentifierOwner
    {
        string Namespace { get; }
    }
}