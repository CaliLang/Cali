using System.Collections.Generic;

namespace Cali.Syntax
{
    public class ClassDeclarationSyntax : IDeclarationContainerSyntax
    {
        public string Name { get; set; }
        public DeclarationModifierSyntax Modifiers { get; set; }

        public ClassDeclarationSyntax() : this("", new DeclarationModifierSyntax())
        {
        }

        public ClassDeclarationSyntax(string name, DeclarationModifierSyntax modifiers)
        {
            Name = name;
            Modifiers = modifiers;
        }
    }
}