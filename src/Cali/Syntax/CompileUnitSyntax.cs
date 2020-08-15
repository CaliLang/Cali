using System.Collections.Generic;

namespace Cali.Syntax
{
    public class CompileUnitSyntax : IStatementSyntax
    {
        public CompileUnitSyntax() : this(null,
        new List<FunctionDeclarationSyntax>(),
        new List<ClassDeclarationSyntax>())
        {
        }

        public IList<FunctionDeclarationSyntax> FunctionDeclarationList { get; }
        public IList<ClassDeclarationSyntax> ClassDeclarationSyntaxList { get; }
        public NamespaceDeclarationSyntax? NamespaceDeclaration { get; }

        public CompileUnitSyntax(NamespaceDeclarationSyntax? namespaceDeclaration,
            IList<FunctionDeclarationSyntax> funcDeclarationList,
            IList<ClassDeclarationSyntax> classDeclarationSyntaxList)
        {
            NamespaceDeclaration = namespaceDeclaration;
            FunctionDeclarationList = funcDeclarationList;
            ClassDeclarationSyntaxList = classDeclarationSyntaxList;
        }
    }
}