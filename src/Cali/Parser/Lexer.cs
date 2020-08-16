using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Cali.Parser
{
    public enum LexerMode
    {
        CompileUnit,
        Class,
        Method,
        StringLiteral
    }

    public class Lexer : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly Stack<LexerMode> _modeStack = new Stack<LexerMode>();
        private readonly bool _shouldDisposeReader;
        private Token? _peekedToken;

        public Lexer(StreamReader reader)
        {
            _reader = reader;
            _modeStack.Push(LexerMode.CompileUnit);
        }

        public Lexer(string code)
        {
            _shouldDisposeReader = true;
            _reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(code)));
            _modeStack.Push(LexerMode.CompileUnit);
        }

        public LexerMode Mode => _modeStack.Peek();

        public void SwitchMode(LexerMode newMode) => _modeStack.Push(newMode);

        public void RevertMode() => _modeStack.Pop();

        public int CurrentLine { get; private set; } = 1;
        public int CurrentColumn { get; private set; }

        public Token NextToken()
        {
            if (_peekedToken != null)
            {
                try
                {
                    return _peekedToken.Value;
                }
                finally
                {
                    _peekedToken = null;
                }
            }

            var availableTokenDefinitions = TokenDescriptor.ByLexerMode[Mode];
            var initialStates = new List<TokenState>(availableTokenDefinitions.Count);
            initialStates.AddRange(
                availableTokenDefinitions.Select(def => new TokenState(this, def)));

            IList<TokenState> currentStates = initialStates;
            var hasAnyMatch = false;
            while (true)
            {
                var peekedChar = _reader.Peek();
                var tokenStates = currentStates
                    .Where(tokenState => tokenState.TryFeedCharacter(peekedChar))
                    .ToList();

                if (tokenStates.Count > 0)
                {
                    _reader.Read(); // commit read byte and move to next

                    currentStates = tokenStates;
                    hasAnyMatch = true;
                    continue;
                }

                if (!hasAnyMatch)
                {
                    // unable to find any recognizable token
                    throw new CaliParseException(
                        $"Unable to recognize input '{(char) peekedChar}' at {CurrentLine}, {CurrentColumn}",
                        CurrentLine, CurrentColumn);
                }

                // pick by precedence order
                var validState = currentStates.First();
                var token = new Token(validState.Descriptor, CurrentLine, CurrentColumn, validState.Value);

                if (validState.Descriptor.Kind == TokenKind.EndOfLine)
                {
                    CurrentLine++;
                    CurrentColumn = 0;
                }
                else
                {
                    CurrentColumn += validState.Value.Length;
                }

                Debug.Print("Token(Name={0}, Value={1})", token.Name, token.Value);

                if (validState.Descriptor == TokenDescriptor.StringLiteralDelimiter)
                {
                    if (_modeStack.Count > 0 && Mode == LexerMode.StringLiteral)
                    {
                        // exiting string literal
                        RevertMode();
                    }
                    else
                    {
                        // entering a string literal
                        SwitchMode(LexerMode.StringLiteral);
                    }
                }

                TokenPosition++;
                return token;
            }
        }

        public int TokenPosition { get; private set; }

        public void Dispose()
        {
            if (_shouldDisposeReader)
                _reader?.Dispose();
        }

        public Token PeekToken()
        {
            if (_peekedToken != null)
                return _peekedToken.Value;

            _peekedToken = NextToken();
            return _peekedToken.Value;
        }
    }
}