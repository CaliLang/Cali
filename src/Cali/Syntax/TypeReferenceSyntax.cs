namespace Cali.Syntax
{
    public class TypeReferenceSyntax : IStatementSyntax
    {
        public string TypeName { get; set; }

        public TypeReferenceSyntax() : this("")
        {
        }

        public TypeReferenceSyntax(string typeName)
        {
            TypeName = typeName;
        }
    }
}