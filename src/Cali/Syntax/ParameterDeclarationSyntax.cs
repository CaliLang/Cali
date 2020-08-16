namespace Cali.Syntax
{
    public class ParameterDeclarationSyntax : IIdentifiableSyntax, ITypedSyntax
    {
        public ParameterDeclarationSyntax()
        {
            Identifier = "";
            Type = new TypeReferenceSyntax();
        }

        public string Identifier { get; set; }
        public TypeReferenceSyntax Type { get; set; }
    }
}