using System;

namespace Cali.Syntax
{
    public class NamespaceDeclarationSyntax : IStatementSyntax
    {
        public NamespaceDeclarationSyntax() : this("")
        {
            
        }
        
        public NamespaceDeclarationSyntax(string fullNamespaceIdentifier)
        {
            this.Identifier = fullNamespaceIdentifier;
        }

        public string Identifier { get; internal set; }
    }
}