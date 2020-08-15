namespace Cali.Syntax
{
    public abstract class LiteralExpressionSyntax<TLiteral> : IExpressionSyntax
    {
        public TLiteral Value { get; }

        protected LiteralExpressionSyntax(TLiteral value)
        {
            Value = value;
        }
    }
}