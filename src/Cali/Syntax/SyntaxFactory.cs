using System.Collections.Generic;

namespace Cali.Syntax
{
    public static class SyntaxFactory
    {
        public static readonly TypeReferenceSyntax UnitTypeReference = new TypeReferenceSyntax("Cali.Unit");

        public static NamespaceDeclarationSyntax NamespaceDeclarationSyntax(string fullNamespaceIdentifier)
        {
            return new NamespaceDeclarationSyntax(fullNamespaceIdentifier);
        }

        public static FunctionDeclarationSyntax FunctionDeclarationSyntax(string name,
            DeclarationModifierSyntax modifiers,
            ICollection<ParameterDeclarationSyntax> parameters,
            TypeReferenceSyntax returnType, ICollection<IStatementSyntax> statements)
        {
            return new FunctionDeclarationSyntax(name, modifiers, parameters, returnType);
        }

        public static CompileUnitSyntax CompileUnitSyntax(NamespaceDeclarationSyntax namespaceDeclarationSyntax,
            IList<FunctionDeclarationSyntax> funcDeclarationSyntaxList,
            IList<ClassDeclarationSyntax> classDeclarationSyntaxList)
        {
            return new CompileUnitSyntax(namespaceDeclarationSyntax,
                funcDeclarationSyntaxList,
                classDeclarationSyntaxList);
        }

        public static DeclarationModifierSyntax ModifierSyntax()
        {
            return new DeclarationModifierSyntax();    
        }

        public static ClassDeclarationSyntax ClassDeclarationSyntax(string name,
            DeclarationModifierSyntax modifiers)
        {
            return new ClassDeclarationSyntax(name, modifiers);
        }

        public static ParameterDeclarationSyntax ParameterDeclarationSyntax(string parameterName,
            TypeReferenceSyntax typeReference)
        {
            return new ParameterDeclarationSyntax()
            {
                Identifier = parameterName,
                Type = typeReference
            };
        }

        public static GenericTypeReferenceSyntax GenericTypeReferenceSyntax(string typeName,
            IList<TypeReferenceSyntax> genericParameters)
        {
            return new GenericTypeReferenceSyntax(typeName, genericParameters);
        }

        public static TypeReferenceSyntax TypeReferenceSyntax(string typeName)
        {
            return new TypeReferenceSyntax(typeName);
        }
    }
}