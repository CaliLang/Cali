namespace Cali.Syntax
{
    public class ReturnStatementSyntax : IExpressionSyntax
    {
        public IExpressionSyntax Expression { get; set; }

        public ReturnStatementSyntax(IExpressionSyntax expression)
        {
            Expression = expression;
        }
    }
}