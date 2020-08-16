using System;
using System.Collections.Generic;
using Cali.Parser;

namespace Cali.Syntax
{
    public class DeclarationModifierSyntax : IStatementSyntax
    {
        public DeclarationModifier Modifier { get; private set; }

        private readonly Dictionary<TokenDescriptor, DeclarationModifier> _mappings =
            new Dictionary<TokenDescriptor, DeclarationModifier>
            {
                {TokenDescriptor.PublicKeyword, DeclarationModifier.Public},
                {TokenDescriptor.PrivateKeyword, DeclarationModifier.Private},
                {TokenDescriptor.ProtectedKeyword, DeclarationModifier.Protected},
                {TokenDescriptor.InternalKeyword, DeclarationModifier.Internal},
                {TokenDescriptor.AbstractKeyword, DeclarationModifier.Abstract},
                {TokenDescriptor.OverrideKeyword, DeclarationModifier.Override},
            };

        public DeclarationModifierSyntax()
        {
            Modifier = DeclarationModifier.None;
        }

        public void Append(DeclarationModifier modifier)
        {
            Modifier |= modifier;
        }

        internal void Append(TokenDescriptor descriptor)
        {
            Modifier |= _mappings[descriptor];
        }

        public bool IsPublic => HasBit(DeclarationModifier.Public);
        public bool IsPrivate => HasBit(DeclarationModifier.Private);
        public bool IsInternal => HasBit(DeclarationModifier.Internal);
        public bool IsAbstract => HasBit(DeclarationModifier.Abstract);
        public bool IsOverride => HasBit(DeclarationModifier.Override);

        private bool HasBit(DeclarationModifier declarationModifier)
        {
            return (Modifier & declarationModifier) > 0;
        }
    }
}