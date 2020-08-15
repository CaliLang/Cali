using System;

namespace Cali
{
    [Flags]
    public enum DeclarationModifier
    {
        None = 0,
        Abstract = 1,
        Final = 1 << 1,
        New = 1 << 3,
        Public = 1 << 4,
        Protected = 1 << 5,
        Internal = 1 << 6,
        Private = 1 << 8,
        InternalProtected = 1 << 9, // the two keywords together are treated as one modifier
        Volatile = 1 << 12,
 
        Extern = 1 << 13,
        Partial = 1 << 14,
        Override = 1 << 18, // used for method binding
 
//        Async = 1 << 20,
//        Ref = 1 << 21, // used only for structs
 
        AccessibilityMask = InternalProtected | Private | Protected | Internal | Public,
        
    }
}