using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cali.Syntax;
using Cali.Utils;

namespace Cali.Parser
{
    public partial class CaliParser1
    {
        private readonly List<DeclarationModifier> modifiersForFunction = new List<DeclarationModifier>
        {
            // Access modifiers
            DeclarationModifier.Public,
            DeclarationModifier.Protected,
            DeclarationModifier.Private,
            DeclarationModifier.Internal, // <-- this is the default

            // Other modifiers
            DeclarationModifier.Override,
            DeclarationModifier.Final
        };

        private readonly List<DeclarationModifier> modifiersForClass = new List<DeclarationModifier>
        {
            // Access modifiers
            DeclarationModifier.Public,
            DeclarationModifier.Protected, // <-- not in top level
            DeclarationModifier.Private, // to current scope (file or class - if nested)
            DeclarationModifier.Internal, // <-- this is the default

            // Other modifiers
            DeclarationModifier.Abstract,
            DeclarationModifier.Final
        };

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

        private CompileUnitSyntax ParseCompileUnit(Lexer lexer)
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax = new NamespaceDeclarationSyntax();
            var funcDeclarationSyntaxList = new List<FunctionDeclarationSyntax>();
            var classDeclarationSyntaxList = new List<ClassDeclarationSyntax>();

            DeclarationModifierSyntax declModifiers = SyntaxFactory.ModifierSyntax();
            while (true)
            {
                // first we should expect the namespace declaration
                var peekedToken = lexer.PeekNextRelevantToken(true);
                if (peekedToken.Is(TokenDescriptor.EndOfFile))
                {
                    break;
                }

                if (peekedToken.Descriptor == TokenDescriptor.NamespaceKeyword)
                {
                    if (namespaceDeclarationSyntax.Identifier.Length == 0)
                    {
                        namespaceDeclarationSyntax = ParseNamespaceDeclarationSyntax(lexer);
                    }
                    else
                    {
                        throw new CaliParseException("Duplicate namespace declaration found", peekedToken);
                    }
                }
                else
                {
                    if (IsDeclModifier(peekedToken.Descriptor))
                    {
                        ParseDeclarationModifiers(lexer, declModifiers);
                        peekedToken = lexer.PeekNextRelevantToken();
                    }

                    var tokenDescriptor = peekedToken.Descriptor;
                    if (tokenDescriptor == TokenDescriptor.FunctionKeyword)
                    {
                        funcDeclarationSyntaxList.Add(ParseFuncDeclarationSyntax(lexer, declModifiers));
                    }
                    else if (tokenDescriptor == TokenDescriptor.ClassKeyword)
                    {
                        classDeclarationSyntaxList.Add(ParseClassDeclarationSyntax(lexer, declModifiers));
                    }
                    else
                    {
                        throw new CaliParseException($"Unexpected token '{peekedToken.Value}'", peekedToken);
                    }
                }
            }

            return SyntaxFactory.CompileUnitSyntax(
                namespaceDeclarationSyntax,
                funcDeclarationSyntaxList,
                classDeclarationSyntaxList);
        }

        private bool IsDeclModifier(TokenDescriptor tokenDescriptor)
        {
            return tokenDescriptor.Kind == TokenKind.Keyword &&
                   (tokenDescriptor == TokenDescriptor.InternalKeyword ||
                    tokenDescriptor == TokenDescriptor.PublicKeyword ||
                    tokenDescriptor == TokenDescriptor.ProtectedKeyword ||
                    tokenDescriptor == TokenDescriptor.PrivateKeyword ||
                    tokenDescriptor == TokenDescriptor.AbstractKeyword
                   );
        }

        private void ParseDeclarationModifiers(Lexer lexer, DeclarationModifierSyntax syntax)
        {
            var modifiers = new List<Token>();
            Token token;
            do
            {
                token = lexer.GetNextRelevantToken();
                modifiers.Add(token);
                token = lexer.PeekNextRelevantToken();
            } while (IsDeclModifier(token.Descriptor));

            modifiers.ForEach(it => syntax.Append(it.Descriptor));
        }

        private ClassDeclarationSyntax ParseClassDeclarationSyntax(Lexer lexer,
            DeclarationModifierSyntax modifiers)
        {
            var token = lexer.GetNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.ClassKeyword);

            ValidateModifiers(modifiersForClass, modifiers, token);

            token = lexer.GetNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.Identifier);

            var name = token.Value;

            token = lexer.PeekNextRelevantToken();
            if (token.Is(TokenDescriptor.LeftBrace))
            {
                lexer.GetNextRelevantToken();
                // class has a body

                lexer.GetNextRelevantToken(true).ExpectedToBe(TokenDescriptor.RightBrace);
            }

            return SyntaxFactory.ClassDeclarationSyntax(name, modifiers);
        }

        private FunctionDeclarationSyntax ParseFuncDeclarationSyntax(Lexer lexer, DeclarationModifierSyntax modifiers)
        {
            // expect func or modifier keyword
            var token = lexer.GetNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.FunctionKeyword);

            ValidateModifiers(modifiersForFunction, modifiers, token);

            token = lexer.GetNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.Identifier);

            var name = token.Value;

            lexer.GetNextRelevantToken().ExpectedToBe(TokenDescriptor.LeftParenthesis);

            var parameterDeclarations = ParseParameterList(lexer);

            lexer.GetNextRelevantToken(true).ExpectedToBe(TokenDescriptor.RightParenthesis);

            TypeReferenceSyntax returnType;
            token = lexer.PeekNextRelevantToken();
            if (token.Is(TokenDescriptor.Arrow))
            {
                lexer.GetNextRelevantToken();

                // now return type  
                returnType = ParseTypeReferenceSyntax(lexer);
            }
            else
            {
                returnType = SyntaxFactory.UnitTypeReference;
            }

            token = lexer.PeekNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.LeftBrace); // expect function body

            // var statements = ParseMethodBody(lexer);

            return SyntaxFactory.FunctionDeclarationSyntax(
                name, modifiers, parameterDeclarations, returnType, new List<IStatementSyntax>());
        }

        private static ICollection<ParameterDeclarationSyntax> ParseParameterList(Lexer lexer)
        {
            var parameterList = new List<ParameterDeclarationSyntax>();

            var token = lexer.PeekNextRelevantToken();
            if (token.IsNot(TokenDescriptor.RightParenthesis))
            {
                do
                {
                    token = lexer.GetNextRelevantToken();
                    token.ExpectedToBe(TokenDescriptor.Identifier);

                    var paramName = token.Value;

                    // expect colon     'argName: ArgType'
                    //                     here ^

                    lexer.GetNextRelevantToken().ExpectedToBe(TokenDescriptor.Colon);

                    var typeReference = ParseTypeReferenceSyntax(lexer);

                    var parameter = SyntaxFactory.ParameterDeclarationSyntax(paramName,
                        typeReference);

                    parameterList.Add(parameter);

                    token = lexer.PeekNextRelevantToken();
                    if (token.Is(TokenDescriptor.Comma))
                    {
                        // there's more parameters
                        lexer.GetNextRelevantToken();
                        token = lexer.PeekNextRelevantToken();
                    }
                } while (token.IsNot(TokenDescriptor.RightParenthesis));
            }

            return parameterList;
        }

        private static TypeReferenceSyntax ParseTypeReferenceSyntax(Lexer lexer)
        {
            var typeName = ParseDottedName(lexer);

            var token = lexer.PeekNextRelevantToken();
            if (token.Is(TokenDescriptor.LeftAngleBracket))
            {
                lexer.GetNextRelevantToken();

                // this is a generic
                var genericParameters = new List<TypeReferenceSyntax>();
                do
                {
                    if (token.Is(TokenDescriptor.Comma)) lexer.GetNextRelevantToken();

                    var genericParam = ParseTypeReferenceSyntax(lexer);
                    genericParameters.Add(genericParam);
                } while (lexer.PeekNextRelevantToken().Is(TokenDescriptor.Comma));

                lexer.GetNextRelevantToken().ExpectedToBe(TokenDescriptor.RightAngleBracket);

                return SyntaxFactory.GenericTypeReferenceSyntax(typeName, genericParameters);
            }

            return SyntaxFactory.TypeReferenceSyntax(typeName);
        }

        private NamespaceDeclarationSyntax ParseNamespaceDeclarationSyntax(Lexer lexer)
        {
            // expect namespace keyword
            var token = lexer.GetNextRelevantToken();
            token.ExpectedToBe(TokenDescriptor.NamespaceKeyword);

            var fullNamespaceIdentifier = ParseDottedName(lexer);

            return SyntaxFactory.NamespaceDeclarationSyntax(fullNamespaceIdentifier);
        }

        private static void ValidateModifiers(ICollection<DeclarationModifier> validModifiers,
            DeclarationModifierSyntax modifiers, Token token)
        {
            // TODO: complete
            if (false)
            {
                throw new CaliParseException($"Invalid modifier '{modifiers.Modifier}' for class", token);
            }
        }

        private static string ParseDottedName(Lexer lexer)
        {
            var token = lexer.PeekNextRelevantToken();

            var dottedName = "";
            do
            {
                token.ExpectedToBe(TokenDescriptor.Identifier);
                lexer.GetNextRelevantToken(); // commit reading identifier
                dottedName += token.Value;

                token = lexer.PeekNextRelevantToken(ignoreWhitespace: false);
                if (token.Is(TokenDescriptor.Dot))
                {
                    dottedName += lexer.GetNextRelevantToken(ignoreWhitespace: false).Value;
                    token = lexer.PeekNextRelevantToken(ignoreWhitespace: false);
                }
                else
                {
                    break;
                }
            } while (token.Is(TokenDescriptor.Identifier));

            return dottedName;
        }
    }

    public class ParseOptions
    {
        public bool Verbose { get; set; }
    }

    internal static class LexerExtensions
    {
        public static Token PeekNextRelevantToken(this Lexer lexer, bool allowLineBreaks = false,
            bool ignoreWhitespace = true)
        {
            return GetNextRelevantTokenInternal(lexer, true, allowLineBreaks, ignoreWhitespace);
        }

        public static Token GetNextRelevantToken(this Lexer lexer, bool allowLineBreaks = false,
            bool ignoreWhitespace = true)
        {
            return GetNextRelevantTokenInternal(lexer, false, allowLineBreaks, ignoreWhitespace);
        }

        private static Token GetNextRelevantTokenInternal(Lexer lexer, bool peekOnly,
            bool allowLineBreaks = false,
            bool ignoreWhitespace = true)
        {
            Token nextToken;
            while (true)
            {
                nextToken = peekOnly ? lexer.PeekToken() : lexer.NextToken();

                if ((ignoreWhitespace && nextToken.Descriptor == TokenDescriptor.Space) ||
                    (ignoreWhitespace && nextToken.Descriptor == TokenDescriptor.Tab) ||
                    nextToken.Descriptor == TokenDescriptor.SingleLineComment ||
                    (allowLineBreaks && nextToken.Descriptor == TokenDescriptor.LineBreak))
                {
                    if (peekOnly)
                    {
                        lexer.NextToken();
                    }

                    continue;
                }

                break;
            }

            return nextToken;
        }
    }
}