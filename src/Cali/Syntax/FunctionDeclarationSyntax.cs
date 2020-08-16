using System.Collections.Generic;

namespace Cali.Syntax
{
    public class FunctionDeclarationSyntax : ITypedSyntax
    {
        public string Name { get; set; }
        public DeclarationModifierSyntax Modifiers { get; set; }
        public ICollection<ParameterDeclarationSyntax> Parameters { get; }
        public TypeReferenceSyntax Type { get; }

        public FunctionDeclarationSyntax() : this("", 
            new DeclarationModifierSyntax(), 
            new List<ParameterDeclarationSyntax>(),
            SyntaxFactory.UnitTypeReference)
        {
        }

        public FunctionDeclarationSyntax(string name,
            DeclarationModifierSyntax modifiers,
            ICollection<ParameterDeclarationSyntax> parameters,
            TypeReferenceSyntax returnType)
        {
            Name = name;
            Modifiers = modifiers;
            Parameters = parameters;
            Type = returnType;
        }

    }
}