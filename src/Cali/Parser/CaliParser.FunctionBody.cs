using System.Collections.Generic;
using Cali.Syntax;
using Cali.Utils;

namespace Cali.Parser
{
    public partial class CaliParser
    {
        private void FunctionParameterDecl(ParsingState<FunctionDeclarationSyntax> parentFunc)
        {
            // parentFunc.ReadAndFork(
            //     p => p.Parameters,
            //     decl =>
            //     {
            //         // decl.Expect()
            //     },
            //     p => p.NextToken().Descriptor == TokenDescriptor.Comma);
        }

        private static IList<IStatementSyntax> ParseMethodBody(Lexer lexer)
        {
            try
            {
                lexer.SwitchMode(LexerMode.Method);

                return ParseStatementBlock(lexer);
            }
            finally
            {
                lexer.RevertMode();
            }
        }

        private static IList<IStatementSyntax> ParseStatementBlock(Lexer lexer)
        {
            var statements = new List<IStatementSyntax>();

            var token = lexer.GetNextRelevantToken(allowLineBreaks: true);
            token.ExpectedToBe(TokenDescriptor.LeftBrace);

            token = lexer.GetNextRelevantToken(allowLineBreaks: true);
            while (token.IsNot(TokenDescriptor.RightBrace))
            {
                token = lexer.GetNextRelevantToken(allowLineBreaks: true);
            }

            return statements;
        }
    }
}