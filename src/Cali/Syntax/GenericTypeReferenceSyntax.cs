using System.Collections.Generic;

namespace Cali.Syntax
{
    public class GenericTypeReferenceSyntax : TypeReferenceSyntax
    {
        public IList<TypeReferenceSyntax> GenericParameters { get; }

        public GenericTypeReferenceSyntax(string typeName, IList<TypeReferenceSyntax> genericParameters)
            : base(typeName)
        {
            GenericParameters = genericParameters;
        }
    }
}