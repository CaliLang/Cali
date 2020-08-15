using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cali.Utils;

namespace Cali.Parser
{
    public enum TokenKind
    {
        Identifier,
        Keyword,
        Whitespace,
        EndOfLine,
        EndOfFile,
        Punctuation,
        Literal,
        Comment
    }

    public interface IFileCoordinatesAware
    {
        int Line { get; }
        int Column { get; }
    }

    public class TokenDescriptor
    {
        private readonly Func<TokenState, bool> _resolver;

        private static readonly List<string> ValidEscapeSequences = new List<string>
        {
            "\\", "\"", "$"
        };

        private static readonly List<int> InvalidStringCharacters = new List<int>
        {
            '\r', '\n', '\"', '$'
        };

        private TokenDescriptor(string name, TokenKind kind, string staticExpression)
        {
            Name = name;
            Kind = kind;
            _resolver = staticExpression.ExactMatch();
            ReportableName = staticExpression;
        }

        private TokenDescriptor(string name, TokenKind kind, Func<TokenState, bool> expression, string reportableName)
        {
            Name = name;
            Kind = kind;
            _resolver = expression;
            ReportableName = reportableName;
        }

        public string Name { get; }

        public TokenKind Kind { get; }

        public String ReportableName { get; }

        internal bool Evaluate(TokenState state)
        {
            return _resolver(state);
        }

        #region Complex expressions

        private static readonly Func<TokenState, bool> LineBreakExpression = state =>
        {
            switch (state.Position)
            {
                case 0:
                    return state.NextChar == '\r' || state.NextChar == '\n';
                case 1:
                    return state.NewValue == "\r\n"; // only this combination is allowed
                default:
                    return false;
            }
        };

        private static readonly Func<TokenState, bool> IdentifierExpression = state =>
        {
            if (state.Position == 0)
            {
                return char.IsLetter((char) state.NextChar) || state.NextChar == '_';
            }

            return char.IsLetterOrDigit((char) state.NextChar); // TODO: add support for unicode characters
        };

        private static readonly Func<TokenState, bool> StringLiteralExpression = state =>
        {
            const string escapingKey = "ESCAPING";

            // check for escaping state first
            var escaping = (bool) state.Data.ComputeIfAbsent(escapingKey, k => false);
            if (escaping)
            {
                if (!ValidEscapeSequences.Contains(((char) state.NextChar).ToString()))
                {
                    throw new CaliParseException(
                        "Unrecognized escape sequence '\\" + (char) state.NextChar + "'",
                        state);
                }

                state.Data[escapingKey] = false;
                return true;
            }

            // starting an escape sequence
            if (state.NextChar == '\\')
            {
                state.Data[escapingKey] = true;
                state.ShouldAddNextChar = false;
                return true;
            }

            return !InvalidStringCharacters.Contains(state.NextChar);
        };

        private static readonly Func<TokenState, bool> BooleanLiteralExpression = state =>
        {
            const string targetLiteralKey = "TARGET_LITERAL";

            if (state.Position == 0)
            {
                switch (state.NextChar)
                {
                    case 't':
                        state.Data[targetLiteralKey] = "true";
                        return true;
                    case 'f':
                        state.Data[targetLiteralKey] = "false";
                        return true;
                    default:
                        return false;
                }
            }

            if (!(state.Data[targetLiteralKey] is string data))
                return false;

            var targetLiteral = data;
            if (state.Position >= targetLiteral.Length) return false;
            return state.NextChar == targetLiteral[state.Position];
        };

        private static readonly Func<TokenState, bool> StringLiteralInsertionAnchorExpression = state =>
        {
            if (state.Position > 0) return IdentifierExpression(state);
            if (state.NextChar != '$') return false;

            state.ShouldAddNextChar = false; // skip adding $ 
            return true;
        };

        private static readonly Func<TokenState, bool> SingleLineCommentExpression = state =>
        {
            const string commentStartedKey = "COMMENT_STARTED";
            var commentStarted = (bool) state.Data.ComputeIfAbsent(commentStartedKey, (k) => false);
            if (commentStarted && state.Position > 1)
            {
                return state.NextChar != '\n' && state.NextChar != '\r';
            }

            if (state.NextChar != '/')
            {
                return false;
            }

            state.ShouldAddNextChar = false;

            if (state.Position > 0)
            {
                // only consider started after second '/'
                state.Data[commentStartedKey] = true;
            }

            return true;
        };

        #endregion

        // Comments
        public static readonly TokenDescriptor SingleLineComment = new TokenDescriptor(
            nameof(SingleLineComment), TokenKind.Comment, SingleLineCommentExpression, "single line comment");

        // Symbols and punctuation
        public static readonly TokenDescriptor EndOfFile = new TokenDescriptor(
            nameof(EndOfFile), TokenKind.EndOfFile, (-1).ExactMatch(), "<EOF>");

        public static readonly TokenDescriptor Space = new TokenDescriptor(
            nameof(Space), TokenKind.Whitespace, " ".Repeatable(), "<whitespace>");

        public static readonly TokenDescriptor Tab = new TokenDescriptor(
            nameof(Tab), TokenKind.Whitespace, "\t".Repeatable(), "<tabs>");

        public static readonly TokenDescriptor LineBreak = new TokenDescriptor(
            nameof(LineBreak), TokenKind.EndOfLine, LineBreakExpression, "<line break>");

        public static readonly TokenDescriptor LeftParenthesis = new TokenDescriptor(
            nameof(LeftParenthesis), TokenKind.Punctuation, "(");

        public static readonly TokenDescriptor RightParenthesis = new TokenDescriptor(
            nameof(RightParenthesis), TokenKind.Punctuation, ")");

        public static readonly TokenDescriptor LeftBrace = new TokenDescriptor(
            nameof(LeftBrace), TokenKind.Punctuation, "{");

        public static readonly TokenDescriptor RightBrace = new TokenDescriptor(
            nameof(RightBrace), TokenKind.Punctuation, "}");

        public static readonly TokenDescriptor LeftBracket = new TokenDescriptor(
            nameof(LeftBracket), TokenKind.Punctuation, "[");

        public static readonly TokenDescriptor RightBracket = new TokenDescriptor(
            nameof(RightBracket), TokenKind.Punctuation, "]");

        public static readonly TokenDescriptor LeftAngleBracket = new TokenDescriptor(
            nameof(LeftAngleBracket), TokenKind.Punctuation, "<");

        public static readonly TokenDescriptor RightAngleBracket = new TokenDescriptor(
            nameof(RightAngleBracket), TokenKind.Punctuation, ">");

        public static readonly TokenDescriptor Dot = new TokenDescriptor(
            nameof(Dot), TokenKind.Punctuation, ".");

        public static readonly TokenDescriptor Comma = new TokenDescriptor(
            nameof(Comma), TokenKind.Punctuation, ",");

        public static readonly TokenDescriptor Arrow = new TokenDescriptor(
            nameof(Arrow), TokenKind.Punctuation, "->");

        public static readonly TokenDescriptor Colon = new TokenDescriptor(
            nameof(Colon), TokenKind.Punctuation, ":");

        public static readonly TokenDescriptor Equal = new TokenDescriptor(
            nameof(Equal), TokenKind.Punctuation, "=");

        public static readonly TokenDescriptor StringLiteralDelimiter = new TokenDescriptor(
            nameof(StringLiteralDelimiter), TokenKind.Punctuation, "\"");

        public static readonly TokenDescriptor StringLiteralInsertionAnchor = new TokenDescriptor(
            nameof(StringLiteralInsertionAnchor), TokenKind.Literal, StringLiteralInsertionAnchorExpression,
            "<NOT REPRESENTABLE>");

        // Literals
        public static readonly TokenDescriptor StringLiteralContent = new TokenDescriptor(
            nameof(StringLiteralContent), TokenKind.Literal, StringLiteralExpression, "string literal");

        public static readonly TokenDescriptor BooleanLiteral = new TokenDescriptor(
            nameof(BooleanLiteral), TokenKind.Literal, BooleanLiteralExpression, "boolean literal");

        // Identifier
        public static readonly TokenDescriptor Identifier = new TokenDescriptor(
            nameof(Identifier), TokenKind.Identifier, IdentifierExpression, "identifier");

        // Keywords
        public static readonly TokenDescriptor NamespaceKeyword = new TokenDescriptor(nameof(NamespaceKeyword),
            TokenKind.Keyword, "namespace");

        public static readonly TokenDescriptor ImportKeyword = new TokenDescriptor(
            nameof(ImportKeyword), TokenKind.Keyword, "import");

        public static readonly TokenDescriptor ClassKeyword = new TokenDescriptor(
            nameof(ClassKeyword), TokenKind.Keyword, "class");

        public static readonly TokenDescriptor PublicKeyword = new TokenDescriptor(
            nameof(PublicKeyword), TokenKind.Keyword, "public");

        public static readonly TokenDescriptor PrivateKeyword = new TokenDescriptor(
            nameof(PrivateKeyword), TokenKind.Keyword, "private");

        public static readonly TokenDescriptor ProtectedKeyword = new TokenDescriptor(
            nameof(ProtectedKeyword), TokenKind.Keyword, "protected");

        public static readonly TokenDescriptor InternalKeyword = new TokenDescriptor(
            nameof(InternalKeyword), TokenKind.Keyword, "internal");

        public static readonly TokenDescriptor OverrideKeyword = new TokenDescriptor(
            nameof(OverrideKeyword), TokenKind.Keyword, "override");

        public static readonly TokenDescriptor FinalKeyword = new TokenDescriptor(
            nameof(FinalKeyword), TokenKind.Keyword, "final");

        public static readonly TokenDescriptor AbstractKeyword = new TokenDescriptor(
            nameof(AbstractKeyword), TokenKind.Keyword, "abstract");

        public static readonly TokenDescriptor FunctionKeyword = new TokenDescriptor(nameof(FunctionKeyword),
            TokenKind.Keyword, "func");

        public static readonly TokenDescriptor LetKeyword = new TokenDescriptor(
            nameof(LetKeyword), TokenKind.Keyword, "let");

        public static readonly TokenDescriptor VarKeyword = new TokenDescriptor(
            nameof(VarKeyword), TokenKind.Keyword, "var");

        public static readonly TokenDescriptor ReturnKeyword = new TokenDescriptor(
            nameof(ReturnKeyword), TokenKind.Keyword, "return");

        private static readonly List<TokenDescriptor> CommonPunctuation = new List<TokenDescriptor>()
        {
            EndOfFile,
            Space,
            LineBreak,
            LeftParenthesis,
            RightParenthesis,
            LeftBracket,
            RightBracket,
            LeftBrace,
            RightBrace,
            LeftAngleBracket,
            RightAngleBracket,
            Dot,
            Comma,
            Arrow,
            Colon,
            Tab,

            // Comments
            SingleLineComment,

            // Literals
            StringLiteralDelimiter,
        };

        public static readonly Dictionary<LexerMode, IList<TokenDescriptor>> ByLexerMode =
            new Dictionary<LexerMode, IList<TokenDescriptor>>
            {
                [LexerMode.CompileUnit] = CommonPunctuation.Union(new List<TokenDescriptor>
                {
                    // Keywords
                    NamespaceKeyword,
                    ImportKeyword,
                    PublicKeyword,
                    InternalKeyword,
                    AbstractKeyword,
                    ClassKeyword,
                    FunctionKeyword,
                    BooleanLiteral,

                    // if it's not any keyword, treat as identifier
                    Identifier,
                }).ToList(),
                [LexerMode.StringLiteral] = new List<TokenDescriptor>()
                {
                    StringLiteralContent,
                    StringLiteralInsertionAnchor,
                    StringLiteralDelimiter,
                },
                [LexerMode.Method] = CommonPunctuation.Union(new List<TokenDescriptor>
                {
                    // Additional symbols/punctuation 
                    Equal,

                    // literal
                    BooleanLiteral,

                    // keywords
                    LetKeyword,
                    VarKeyword,
                    ReturnKeyword,

                    // if it's not any keyword, treat as identifier
                    Identifier
                }).ToList()
            };
    }

    [DebuggerDisplay("Token(Type={Descriptor.Name}, Value={Value})")]
    public struct Token : IFileCoordinatesAware
    {
        public Token(TokenDescriptor descriptor, int line, int column, string value)
        {
            Descriptor = descriptor;
            Line = line;
            Column = column;
            Value = value;
        }

        public TokenDescriptor Descriptor { get; }

        public int Line { get; }

        public int Column { get; }

        public string Value { get; }

        public string Name => Descriptor.Name;
    }

    [DebuggerDisplay("TokenState(Type={Descriptor.Name}, Position={Position}, Value={Value})")]
    internal class TokenState : IFileCoordinatesAware
    {
        private readonly Lexer _lexer;
        private bool _hasEverMatched;

        internal TokenState(Lexer lexer, TokenDescriptor descriptor)
        {
            _lexer = lexer;
            Descriptor = descriptor;
        }

        public TokenDescriptor Descriptor { get; }

        public Dictionary<string, object> Data { get; } = new Dictionary<string, object>();

        public string Value { get; private set; } = "";

        public string NewValue { get; private set; } = "";

        public int NextChar { get; set; }

        public bool ShouldAddNextChar { get; set; }

        public int Position { get; private set; }

        public int Line => _lexer.CurrentLine;

        public int Column => _lexer.CurrentColumn + Position;

        public bool TryFeedCharacter(int peekedChar)
        {
            NextChar = peekedChar;
            ShouldAddNextChar = true;

            NewValue = Value + (char) peekedChar;
            var keepGoing = Descriptor.Evaluate(this);
            _hasEverMatched = _hasEverMatched || keepGoing;

            Position++;

            if (keepGoing && ShouldAddNextChar)
            {
                Value += (char) peekedChar;
            }

            return keepGoing;
        }
    }

    internal static class TokenDefinitionHelpers
    {
        public static Func<TokenState, bool> ExactMatch(this int fixedValue)
        {
            return state =>
            {
                if (state.Position > 0) return false;
                return fixedValue == state.NextChar;
            };
        }

        public static Func<TokenState, bool> ExactMatch(this string fixedContent)
        {
            return state =>
            {
                if (state.Position >= fixedContent.Length) return false;
                return (int) fixedContent[state.Position] == state.NextChar;
            };
        }

        public static Func<TokenState, bool> Repeatable(this string repStr)
        {
            return state =>
            {
                var index = state.Position % repStr.Length;
                return (int) repStr[index] == state.NextChar;
            };
        }
    }
}