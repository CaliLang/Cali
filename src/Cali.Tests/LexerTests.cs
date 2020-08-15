using Cali.Parser;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Cali.Tests
{
    public class LexerTests
    {
        private static void AssertToken(Token token, TokenDescriptor tokenDescriptor, string value)
        {
            Assert.Equal(tokenDescriptor, token.Descriptor);
            Assert.Equal(value, token.Value);
        }

        private static void AssertToken(Token token, TokenDescriptor tokenDescriptor, string value, int line, int col)
        {
            AssertToken(token, tokenDescriptor, value);
            Assert.Equal(line, token.Line);
            Assert.Equal(col, token.Column);
        }

        [Fact]
        public void PeekVsReadToken()
        {
            using var reader = "namespace Cali".ToStreamReader();
            var lexer = new Lexer(reader);
            
            Assert.Equal(TokenDescriptor.NamespaceKeyword, lexer.PeekToken().Descriptor);
            Assert.Equal(TokenDescriptor.NamespaceKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.PeekToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Identifier, lexer.PeekToken().Descriptor);
            Assert.Equal(TokenDescriptor.Identifier, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.PeekToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexEmptyFile()
        {
            using var reader = "".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexInvalidToken()
        {
            using var reader = "\nclass ~~".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.ClassKeyword, "class", 2, 0);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            var exception = Assert.Throws<CaliParseException>(() => lexer.NextToken().Descriptor);
            Assert.Equal(2, exception.Line);
            Assert.Equal(6, exception.Column);
        }

        [Fact]
        public void LexLineBreaks()
        {
            using var reader = "\n\n\n ".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexWindowsLineBreaks()
        {
            using var reader = "\r\n\r\n\r\n ".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
            Assert.Equal(4, lexer.CurrentLine);
        }

        [Fact]
        public void LexMultipleSpacesAsOne()
        {
            using var reader = "namespace      Cali ".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.NamespaceKeyword, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Space, "      ", 1, 9);
            Assert.Equal(TokenDescriptor.Identifier, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Space, " ", 1, 19);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexSingleLineComments()
        {
            using var reader = "namespace Cali\n// Comment after namespace\n".ToStreamReader();

            var lexer = new Lexer(reader);
            Assert.Equal(TokenDescriptor.NamespaceKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Identifier, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.SingleLineComment, " Comment after namespace");
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexNamespaceSyntax()
        {
            using var reader = "namespace Cali".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.NamespaceKeyword, "namespace", 1, 0);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Cali", 1, 10);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexImportSyntax()
        {
            using var reader = "import Cali\nimport Cali.Runtime\nimport System.Collections.Generic".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.ImportKeyword, "import");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Cali");
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);

            Assert.Equal(TokenDescriptor.ImportKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Cali");
            Assert.Equal(TokenDescriptor.Dot, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Runtime");
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);

            Assert.Equal(TokenDescriptor.ImportKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "System");
            Assert.Equal(TokenDescriptor.Dot, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Collections");
            Assert.Equal(TokenDescriptor.Dot, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Generic");

            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexFunctionDeclarationSyntax()
        {
            using var reader = "func Kamehameha()".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.FunctionKeyword, "func", 1, 0);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Kamehameha", 1, 5);
            Assert.Equal(TokenDescriptor.LeftParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexClassDeclarationSyntax()
        {
            using var reader = "class Jedi".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.ClassKeyword, "class", 1, 0);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Jedi", 1, 6);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexPublicAbstractClassDeclarationSyntax()
        {
            using var reader = "public abstract class Jedi".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.PublicKeyword, "public");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.AbstractKeyword, "abstract");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.ClassKeyword, "class");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Jedi");
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexEmptyClassDefinitionSyntax()
        {
            using var reader = "class Padawan {\n}\n".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.ClassKeyword, "class", 1, 0);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Padawan", 1, 6);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LeftBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexEmptyFunctionDefinitionSyntax()
        {
            using var reader = "func UseLightSaber() {\n}\n".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.FunctionKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "UseLightSaber", 1, 5);
            Assert.Equal(TokenDescriptor.LeftParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LeftBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexFunctionDefinitionSyntaxWithParametersAndRetVal()
        {
            using var reader = "func KillStormTrooper(saber: LightSaber) -> Bool { }"
                .ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.FunctionKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "KillStormTrooper");
            Assert.Equal(TokenDescriptor.LeftParenthesis, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "saber");
            Assert.Equal(TokenDescriptor.Colon, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "LightSaber");
            Assert.Equal(TokenDescriptor.RightParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Arrow, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Bool");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LeftBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }
        
        [Fact]
        public void LexFunctionDefinitionSyntaxWithGenericParameter()
        {
            using var reader = "func KillStormTrooper(saber: Array<Int>) -> Bool { }"
                .ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.FunctionKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "KillStormTrooper");
            Assert.Equal(TokenDescriptor.LeftParenthesis, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "saber");
            Assert.Equal(TokenDescriptor.Colon, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Array");
            Assert.Equal(TokenDescriptor.LeftAngleBracket, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Int");
            Assert.Equal(TokenDescriptor.RightAngleBracket, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Arrow, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Bool");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.LeftBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightBrace, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexBasicMethodCall()
        {
            using var reader = "Console.WriteLine(\"Use the force\")".ToStreamReader();
            var lexer = new Lexer(reader);

            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "Console", 1, 0);
            Assert.Equal(TokenDescriptor.Dot, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "WriteLine", 1, 8);
            Assert.Equal(TokenDescriptor.LeftParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralContent, "Use the force", 1, 19);
            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.RightParenthesis, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexStringInterpolation()
        {
            using var reader = "\"My name is $jediName, and I'm from $planet\"".ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralContent, "My name is ");
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralInsertionAnchor, "jediName");
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralContent, ", and I'm from ");
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralInsertionAnchor, "planet");
            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexStringWithValidEscapeSequences()
        {
            using var reader = ("\"My name is \\\"Obi Wan\\\" and I'm a Jedi. " +
                                "A New Hope made \\$775.4 million at the box office!. " +
                                "No StarWars trivia to use \\\\ (backslash character)\"").ToStreamReader();

            const string stringToValidate = "My name is \"Obi Wan\" and I'm a Jedi. " +
                                            "A New Hope made $775.4 million at the box office!. " +
                                            "No StarWars trivia to use \\ (backslash character)";
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.StringLiteralContent, stringToValidate);
            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.EndOfFile, lexer.NextToken().Descriptor);
        }

        [Fact]
        public void LexStringWithInvalidEscapeSequence()
        {
            using var reader = ("\"My account is \\@jediaccount. Follow me!\"").ToStreamReader();
            var lexer = new Lexer(reader);

            Assert.Equal(TokenDescriptor.StringLiteralDelimiter, lexer.NextToken().Descriptor);
            var exception = Assert.Throws<CaliParseException>(() => lexer.NextToken().Descriptor);
            Assert.Equal(1, exception.Line);
            Assert.Equal(16, exception.Column);
        }

        [Fact]
        public void LexBooleanLiteral()
        {
            using var reader = "let x = true\nlet y = false".ToStreamReader();
            var lexer = new Lexer(reader);
            lexer.SwitchMode(LexerMode.Method);
            
            Assert.Equal(TokenDescriptor.LetKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "x");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Equal, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.BooleanLiteral, "true");
            
            Assert.Equal(TokenDescriptor.LineBreak, lexer.NextToken().Descriptor);
            
            Assert.Equal(TokenDescriptor.LetKeyword, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.Identifier, "y");
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Equal, lexer.NextToken().Descriptor);
            Assert.Equal(TokenDescriptor.Space, lexer.NextToken().Descriptor);
            AssertToken(lexer.NextToken(), TokenDescriptor.BooleanLiteral, "false");
        }
    }
}