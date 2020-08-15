using System;

namespace Cali.Syntax
{
    public abstract class AbstractVariableDeclarationSyntax : IStatementSyntax
    {
        public string VariableName { get; }
        
        public IExpressionSyntax? Initializer { get; }

        public AbstractVariableDeclarationSyntax(string variableName, IExpressionSyntax? initializer)
        {
            VariableName = variableName;
            Initializer = initializer;
        }
    }
}