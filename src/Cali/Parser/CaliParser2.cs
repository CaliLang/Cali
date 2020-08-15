
using System;
using Cali.Syntax;

namespace Cali.Parser
{

    public class CaliParser2
    {

        public CompileUnitSyntax ParseFile2(string filePath, ParserOptions? options = null)
        {

(new Func<ParsingState<CompileUnitSyntax>, ParsingState<CompileUnitSyntax>>(ParseComments)).ZeroOrMore();

            return Parse<CompileUnitSyntax>()
            .OneOf(
                // ParseComments.ZeroOrMore(),
                ParsingStateExtensions.ZeroOrMore(ParseComments),
                ZeroOrMore(ParseImportStatements),
                ZeroOrOne(ParseNamespaceDeclaration),
                ZeroOrMore(ParseFunctionDecl),
                ZeroOrMore(ParseClassDecl)
            ).Syntax;
        }

        internal ParsingState<T> Parse<T>() where T : notnull, IStatementSyntax, new()
        {
            return new ParsingState<T>();
        }

        private ParsingState<CompileUnitSyntax> ParseComments(ParsingState<CompileUnitSyntax> state) 
        {
            return state;
        }
    }

    internal class ParsingState<T> where T : notnull, IStatementSyntax, new()
    {
        public T Syntax { get; private set; }

        public ParsingState() {
            Syntax = new T();
        }
    }

    internal static class ParsingStateExtensions {
        public static ParsingState<T> OneOf<T>(this ParsingState<T> state,
            params ParseDelegate<T>[] delegates) where T: notnull, IStatementSyntax, new() {
                return state;
        }

        public static ParseDelegate<T> ZeroOrMore<T>(this ParseDelegate<T> del)
        where T: notnull, IStatementSyntax, new()   {
            return del;
        }
    }

    internal delegate ParsingState<T> ParseDelegate<T>(ParsingState<T> state) where T: notnull, IStatementSyntax, new();
}