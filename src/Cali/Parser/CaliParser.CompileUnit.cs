using System.Collections.Generic;
using Cali.Syntax;

namespace Cali.Parser
{
    public partial class CaliParser
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

        public CompileUnitSyntax ParseCompileUnit(Lexer lexer)
        {
            return ParseInto<CompileUnitSyntax>(lexer)
                .AnyNumberOf(Comments, LineBreaks)
                .AtMostOne(NamespaceDecl)
                .AnyNumberOf(Comments, LineBreaks)
                .AnyNumberOf(ImportStatement, LineBreaks)
                .AnyNumberOf(
                    DeclModifiers,
                    FunctionDecl,
                    ClassDecl
                )
                .Syntax;
        }

        private void ImportStatement(ParsingState<CompileUnitSyntax> state)
        {
            state.WhenNextTokenIs(TokenDescriptor.ImportKeyword, it =>
            {
                it.ReadAndFork(s => s.NamespaceImports,
                    im =>
                    {
                        im.Expect(TokenDescriptor.Identifier,
                                (p, tok) => p.Identifier = tok.Value)
                            .AtLeastOne(TokenDescriptor.LineBreak, TokenDescriptor.EndOfFile);
                    });
            });
        }

        private void NamespaceDecl(ParsingState<CompileUnitSyntax> state)
        {
            state.WhenNextTokenIs(TokenDescriptor.NamespaceKeyword, it =>
            {
                it.ReadAndFork(s => s.NamespaceDeclaration,
                    nsDecl =>
                    {
                        nsDecl.Expect(TokenDescriptor.Identifier,
                                (ns, tok) => ns.Identifier = tok.Value)
                            .AnyNumberOf(nsDecl =>
                            {
                                nsDecl.WhenNextTokenIs(TokenDescriptor.Dot, next =>
                                {
                                    next.Read(); // ignore
                                    next.Expect(TokenDescriptor.Identifier,
                                        (ns, tok) => ns.Identifier += "." + tok.Value);
                                });
                            })
                            .AtLeastOne(TokenDescriptor.LineBreak, TokenDescriptor.EndOfFile);
                    }
                );
            });
        }

        private void FunctionDecl(ParsingState<CompileUnitSyntax> state)
        {
            state.WhenNextTokenIs(TokenDescriptor.FunctionKeyword, it =>
            {
                it.ReadAndFork(s => s.FunctionDeclarationList,
                    fnDecl =>
                    {
                        fnDecl.MaybePopSyntax<DeclarationModifierSyntax>(
                                (fn, decl) => fn.Modifiers = decl)
                            .Expect(TokenDescriptor.Identifier,
                                (fn, tok) => fn.Name = tok.Value)
                            .Expect(TokenDescriptor.LeftParenthesis)
                            // .AnyNumberOf(FunctionParameterDecl)
                            .Expect(TokenDescriptor.RightParenthesis)
                            .WhenNextTokenIs(TokenDescriptor.Arrow, functionType =>
                            {
                                functionType.ReadAndFork(f => f.Type,
                                    x => x.ExactlyOne(TypeReference)
                                );
                            });
                    });
            });
        }

        private void ClassDecl(ParsingState<CompileUnitSyntax> state)
        {
            state.WhenNextTokenIs(TokenDescriptor.ClassKeyword, it =>
            {
                it.ReadAndFork(s => s.ClassDeclarationSyntaxList,
                    classDecl =>
                    {
                        classDecl.Expect(TokenDescriptor.Identifier,
                                (cl, tok) => cl.Name = tok.Value)
                            .MaybePopSyntax<DeclarationModifierSyntax>(
                                (cl, decl) => cl.Modifiers = decl);
                    });
            });
        }
    }
}