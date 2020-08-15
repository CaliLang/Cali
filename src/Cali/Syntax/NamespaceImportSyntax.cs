namespace Cali.Syntax
{
    public class NamespaceImportSyntax : IIdentifierOwner
    {
        public string Identifier { get; }

        public NamespaceImportSyntax(string identifier)
        {
            this.Identifier = identifier;
        }
    }
}