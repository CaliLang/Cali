using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cali.Syntax;

namespace Cali.Parser
{
    public partial class CaliParser
    {
        public CompileUnitSyntax ParseFile(string filePath, ParseOptions? options = null)
        {
            using var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            var lexer = new Lexer(reader);

            return ParseCompileUnit(lexer);
        }

        public CompileUnitSyntax ParseString(string code, ParseOptions? options = null)
        {
            var lexer = new Lexer(code);

            return ParseCompileUnit(lexer);
        }
        
        private ParsingState<T> ParseInto<T>(Lexer lexer) where T : IStatementSyntax, new()
        {
            return new ParsingState<T>(lexer);
        }

        private void Comments<T>(ParsingState<T> state) where T : ICommentContainerSyntax, new()
        {
        }

        private void LineBreaks<T>(ParsingState<T> state) where T : ICommentContainerSyntax, new()
        {
            state.WhenNextTokenIs(TokenDescriptor.LineBreak, it => it.Read());
        }

        private void TypeReference(ParsingState<TypeReferenceSyntax> state)
        {
            state.Expect(TokenDescriptor.Identifier, (syntax, token) =>
                syntax.TypeName = token.Value);
        }

        public List<DeclarationModifier> ModifiersForFunction => modifiersForFunction;

        private void DeclModifiers<TDeclType>(ParsingState<TDeclType> state)
            where TDeclType : IDeclarationContainerSyntax, new()
        {
            state.WhenNextTokenIs(new List<TokenDescriptor>
            {
                TokenDescriptor.PublicKeyword,
                TokenDescriptor.PrivateKeyword,
                TokenDescriptor.ProtectedKeyword,
                TokenDescriptor.InternalKeyword,
                TokenDescriptor.OverrideKeyword,
                TokenDescriptor.AbstractKeyword
            }, it =>
            {
                it.ApplyToFloatingSyntax<DeclarationModifierSyntax>(
                    (decl, tok) => decl.Append(tok.Descriptor));
            });
        }
    }

    internal class ParsingState<T> where T : IStatementSyntax, new()
    {
        private readonly Lexer _lexer;
        private Stack<IStatementSyntax> _floatingSyntaxStack = new Stack<IStatementSyntax>();

        public T Syntax { get; private set; }

        public ParsingState(Lexer lexer)
        {
            _lexer = lexer;
            Syntax = new T();
        }

        /// <summary>
        /// Will parse using these delegates until none of them successfully parsed anything
        /// </summary>
        public ParsingState<T> AnyNumberOf(params ParseDelegate<T>[] delegates)
        {
            // if the tokens read have changed, that means something was read. Therefore keep going
            var startingPosition = _lexer.TokenPosition;
            var prevPosition = -1;
            while (startingPosition != prevPosition)
            {
                foreach (var parseDelegate in delegates)
                {
                    parseDelegate(this);
                }

                prevPosition = startingPosition;
                startingPosition = _lexer.TokenPosition;
            }

            return this;
        }

        /// <summary>
        /// Will go through these once, without checking whether they read something or not
        /// </summary>
        public ParsingState<T> AtMostOne(params ParseDelegate<T>[] delegates)
        {
            foreach (var parseDelegate in delegates)
            {
                parseDelegate(this);
            }

            return this;
        }
        
        public ParsingState<T> AtLeastOne(params TokenDescriptor[] descriptors)
        {
            var found = false;
            if (descriptors.Any(desc => desc == _lexer.PeekToken().Descriptor))
            {
                found = true;
                Read();
            }

            if (!found)
            {
                throw new CaliParseException($"Expecting one of {descriptors} but found {_lexer.PeekToken().Value}", 
                    _lexer.PeekToken());
            }

            return this;
        }
        
        public ParsingState<T> ExactlyOne(ParseDelegate<T> action)
        {
            action(this);
            return this;
        }

        public ParsingState<T> Expect(TokenDescriptor descriptor, Action<ParsingState<T>>? action = null)
        {
            if (InvokeIfMatching(descriptor, action)) return this;

            var actualToken = _lexer.PeekToken();
            throw new CaliParseException($"Expected '{descriptor.ReportableName}' but found '{actualToken.Value}'", 0,
                0);
        }

        public ParsingState<T> WhenNextTokenIs(TokenDescriptor descriptor, Action<ParsingState<T>> action)
        {
            // Ignore white spaces
            InvokeIfMatching(descriptor, action);

            return this;
        }

        public ParsingState<T> WhenNextTokenIs(IEnumerable<TokenDescriptor> descriptors, Action<ParsingState<T>> action)
        {
            // Ignore white spaces
            foreach (var descriptor in descriptors)
            {
                InvokeIfMatching(descriptor, action);
            }

            return this;
        }

        public ParsingState<T> Expect(TokenDescriptor descriptor, Action<T, Token> action)
        {
            // Ignore white spaces
            while (_lexer.PeekToken().Descriptor == TokenDescriptor.Space) _lexer.NextToken();

            if (_lexer.PeekToken().Descriptor == descriptor)
            {
                action(Syntax, _lexer.NextToken());
            }
            else
            {
                throw new CaliParseException(
                    $"Expecting '{descriptor.ReportableName}' but found '{_lexer.PeekToken().Value}'", 
                    _lexer.PeekToken());
            }

            return this;
        }
        
        
        public ParsingState<T> ApplyToFloatingSyntax<TSyntax>(Action<TSyntax, Token> action) where TSyntax: class, IStatementSyntax, new()
        {
            if (!StackHeadIs<TSyntax>())
            {
                // add value if missing
                _floatingSyntaxStack.Push(new TSyntax());
            }

            var syntax = (_floatingSyntaxStack.Peek() as TSyntax)!;

            action(syntax, _lexer.NextToken());
            return this;
        }

        public ParsingState<T> PopSyntax<TSyntax>(Action<T, TSyntax> func) where TSyntax : class
        {
            if (StackHeadIs<TSyntax>())
            {
                func(Syntax, (_floatingSyntaxStack.Pop() as TSyntax)!);
            }
            
            return this;
        }
        
        public ParsingState<T> MaybePopSyntax<TSyntax>(Action<T, TSyntax> func) where TSyntax : class
        {
            return StackHeadIs<TSyntax>() ? PopSyntax(func) : this;
        }

        public Token Read()
        {
            return _lexer.NextToken();
        }

        public ParsingState<T> ReadAndFork<TChild>(Func<T, TChild> mapper,
            Action<ParsingState<TChild>> action) where TChild : IStatementSyntax, new()
        {
            Read();
            
            var childParsingState = new ChildParsingState<TChild, T>(_lexer, this, mapper.Invoke(Syntax));
            action(childParsingState);

            return this;
        }

        public ParsingState<T> ReadAndFork<TChild>(Func<T, ICollection<TChild>> mapper, 
            Action<ParsingState<TChild>> action,
            Predicate<ParsingState<T>>? repeatWhen = null)
            where TChild : IStatementSyntax, new()
        {
            do
            {
                Read();
                var childParsingState = new CollectionChildParsingState<TChild, T>(
                    _lexer, this, mapper.Invoke(Syntax));
                action(childParsingState);

                childParsingState.Join();
            } while (repeatWhen != null && repeatWhen(this));
          
            return this;
        }

        private class ChildParsingState<TChild, TParent> : ParsingState<TChild>
            where TChild : IStatementSyntax, new()
            where TParent : IStatementSyntax, new()
        {
            private readonly ParsingState<TParent> _parent;

            internal ChildParsingState(Lexer lexer,
                ParsingState<TParent> parent,
                TChild syntax) : base(lexer)
            {
                _parent = parent;
                Syntax = syntax;
                this._floatingSyntaxStack = parent._floatingSyntaxStack;
            }
        }

        private class CollectionChildParsingState<TChild, TParent> : ParsingState<TChild>
            where TChild : IStatementSyntax, new()
            where TParent : IStatementSyntax, new()
        {
            private readonly ParsingState<TParent> _parent;
            private readonly ICollection<TChild> _collection;

            internal CollectionChildParsingState(Lexer lexer,
                ParsingState<TParent> parent,
                ICollection<TChild> collection) : base(lexer)
            {
                _parent = parent;
                _collection = collection;
                this._floatingSyntaxStack = parent._floatingSyntaxStack;
            }

            public void Join()
            {
                _collection.Add(Syntax);
            }
        }

        private bool StackHeadIs<TSyntax>() where TSyntax : class
        {
            return _floatingSyntaxStack.Count > 0 && _floatingSyntaxStack.Peek() is TSyntax;
        }
        
        private bool InvokeIfMatching(TokenDescriptor descriptor, Action<ParsingState<T>>? action)
        {
            // Ignore white spaces
            while (_lexer.PeekToken().Descriptor == TokenDescriptor.Space) _lexer.NextToken();

            if (_lexer.PeekToken().Descriptor == descriptor)
            {
                action?.Invoke(this);
                return true;
            }

            return false;
        }

        public Token NextToken()
        {
            return _lexer.PeekToken();
        }
    }

    internal static class ParsingStateHelpers
    {
        public static ParseDelegate<T> ZeroOrMore<T>(ParseDelegate<T> del)
            where T : IStatementSyntax, new()
        {
            return del;
        }
    }

    internal delegate void ParseDelegate<T>(ParsingState<T> state) where T : IStatementSyntax, new();
}