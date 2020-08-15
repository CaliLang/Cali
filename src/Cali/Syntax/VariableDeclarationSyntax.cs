namespace Cali.Syntax
{
    public class VariableDeclarationSyntax : AbstractVariableDeclarationSyntax
    {
        public VariableDeclarationSyntax(string variableName, IExpressionSyntax initializer) 
            : base(variableName, initializer)
        {
        }
    }
}