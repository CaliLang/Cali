using System.Collections.Generic;

namespace Cali.Syntax
{
    public class FunctionDeclarationSyntax
    {
        public string Name { get; }
        public IEnumerable<ParameterDeclarationSyntax> Parameters { get; }
        public TypeReferenceSyntax ReturnType { get; }

        public FunctionDeclarationSyntax(string name,
            DeclarationModifierSyntax modifiers,
            IEnumerable<ParameterDeclarationSyntax> parameters,
            TypeReferenceSyntax returnType)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
        }
    }
}