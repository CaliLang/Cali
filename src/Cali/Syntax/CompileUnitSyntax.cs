using System.Collections.Generic;

namespace Cali.Syntax
{
    public class CompileUnitSyntax : ICommentContainerSyntax, IDeclarationContainerSyntax
    {
        public CompileUnitSyntax() : this(new NamespaceDeclarationSyntax(), 
        new List<FunctionDeclarationSyntax>(),
        new List<ClassDeclarationSyntax>())
        {
        }

        public ICollection<FunctionDeclarationSyntax> FunctionDeclarationList { get; }
        public ICollection<ClassDeclarationSyntax> ClassDeclarationSyntaxList { get; }
        public ICollection<CommentSyntax> Comments { get; } = new List<CommentSyntax>();
        public NamespaceDeclarationSyntax NamespaceDeclaration { get; }
        public ICollection<NamespaceImportSyntax> NamespaceImports { get; } = new List<NamespaceImportSyntax>();

        public CompileUnitSyntax(NamespaceDeclarationSyntax namespaceDeclaration,
            ICollection<FunctionDeclarationSyntax> funcDeclarationList,
            ICollection<ClassDeclarationSyntax> classDeclarationSyntaxList)
        {
            NamespaceDeclaration = namespaceDeclaration;
            FunctionDeclarationList = funcDeclarationList;
            ClassDeclarationSyntaxList = classDeclarationSyntaxList;
        }
    }
}