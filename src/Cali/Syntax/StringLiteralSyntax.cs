namespace Cali.Syntax
{
    public class StringLiteralSyntax : LiteralExpressionSyntax<string>
    {
        public StringLiteralSyntax(string value) : base(value)
        {
        }
    }
}