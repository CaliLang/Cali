using System;

namespace Cali.Syntax
{
    public class NamespaceDeclarationSyntax
    {
        public NamespaceDeclarationSyntax(string fullNamespaceIdentifier)
        {
            this.Identifier = fullNamespaceIdentifier;
        }

        public string Identifier { get; set; }
    }
}