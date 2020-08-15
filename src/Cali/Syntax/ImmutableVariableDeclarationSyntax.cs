namespace Cali.Syntax
{
    public class ImmutableVariableDeclarationSyntax : AbstractVariableDeclarationSyntax
    {
        public ImmutableVariableDeclarationSyntax(string variableName, IExpressionSyntax initializer) 
            : base(variableName, initializer)
        {
        }
    }
}