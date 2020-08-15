using System.Collections.Generic;

namespace Cali.Syntax
{
    public class ClassDeclarationSyntax
    {
        public string Name { get; }
        public DeclarationModifierSyntax Modifiers { get; }

        public ClassDeclarationSyntax(string name, DeclarationModifierSyntax modifiers)
        {
            Name = name;
            Modifiers = modifiers;
        }
    }
}