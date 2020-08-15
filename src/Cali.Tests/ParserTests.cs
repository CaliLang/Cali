using System.Linq;
using Cali.Parser;
using Xunit;

namespace Cali.Tests
{
    public class ParserTests
    {
        [Fact]
        public void ParseNamespaceDeclSimple()
        {
            var parser = new CaliParser();

            var compileUnitSyntax = parser.ParseString("namespace Simple");
            var namespaceDeclaration = compileUnitSyntax.NamespaceDeclaration;

            Assert.NotNull(namespaceDeclaration);
            Assert.Equal("Simple", namespaceDeclaration!.Identifier);
        }

        [Fact]
        public void ParseNamespaceDeclComplex()
        {
            var parser = new CaliParser();

            var compileUnitSyntax = parser.ParseString("\n\n    namespace More.Complex.Test");
            var namespaceDeclaration = compileUnitSyntax.NamespaceDeclaration;

            Assert.NotNull(namespaceDeclaration);
            Assert.Equal("More.Complex.Test", namespaceDeclaration!.Identifier);
        }

        [Fact]
        public void ParseNamespaceWithScopeIsInvalid()
        {
            var parser = new CaliParser();

            Assert.Throws<CaliParseException>(() =>
                parser.ParseString("\n\n    namespace ScopeNamespace {\n}"));
        }

        [Fact]
        public void ParseClassDefinition()
        {
            var parser = new CaliParser();

            var compileUnitSyntax = parser.ParseString(@"
namespace MyLibrary

class MyFirstClass {
}
");
            var namespaceDeclaration = compileUnitSyntax.NamespaceDeclaration;

            Assert.NotNull(namespaceDeclaration);
            Assert.Equal("MyLibrary", namespaceDeclaration!.Identifier);

            Assert.True(compileUnitSyntax.FunctionDeclarationList.Count == 0);
            Assert.True(compileUnitSyntax.ClassDeclarationSyntaxList.Count > 0);
            Assert.Equal("MyFirstClass", compileUnitSyntax.ClassDeclarationSyntaxList.First().Name);
        }

        [Fact]
        public void ParseInternalAbstractClassDefinition()
        {
            var parser = new CaliParser();

            var compileUnitSyntax = parser.ParseString(@"
namespace MyLibrary

internal abstract class MyFirstClass {
}
");
            var namespaceDeclaration = compileUnitSyntax.NamespaceDeclaration;

            Assert.NotNull(namespaceDeclaration);
            Assert.Equal("MyLibrary", namespaceDeclaration?.Identifier);

            Assert.True(compileUnitSyntax.FunctionDeclarationList.Count == 0);
            Assert.True(compileUnitSyntax.ClassDeclarationSyntaxList.Count > 0);
            var classDecl = compileUnitSyntax.ClassDeclarationSyntaxList.First();
            Assert.Equal("MyFirstClass", classDecl.Name);
            Assert.True(classDecl.Modifiers.IsInternal);
            Assert.True(classDecl.Modifiers.IsAbstract);
            Assert.False(classDecl.Modifiers.IsPrivate);
        }

        [Fact]
        public void ParseFunctionDefinition()
        {
            var parser = new CaliParser();

            var compileUnitSyntax = parser.ParseString(@"
namespace MyLibrary

func main(args: Array<String>) -> Int {
}
");
            var namespaceDeclaration = compileUnitSyntax.NamespaceDeclaration;

            Assert.NotNull(namespaceDeclaration);
            Assert.Equal("MyLibrary", namespaceDeclaration!.Identifier);

            Assert.True(compileUnitSyntax.ClassDeclarationSyntaxList.Count == 0);
            Assert.True(compileUnitSyntax.FunctionDeclarationList.Count > 0);
            var func = compileUnitSyntax.FunctionDeclarationList.First();
            Assert.Equal("main", func.Name);
        }
    }
}