namespace Cali.Syntax
{
    public class NamespaceImportSyntax : IStatementSyntax, IIdentifiableSyntax
    {
        public string Identifier { get; set; }

        public NamespaceImportSyntax(string identifier)
        {
            this.Identifier = identifier;
        }

        public NamespaceImportSyntax() : this("")
        {
        }
    }
}