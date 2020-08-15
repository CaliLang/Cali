using System;
using System.Collections.Generic;
using System.Linq;
using Cali.Parser;

namespace Cali.Utils
{
    public static class DictionaryUtils
    {
        public static TValue ComputeIfAbsent<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> provider) where TKey : notnull
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = provider(key);
            }

            return dictionary[key];
        }
    }

    public static class TokenUtils
    {
        internal static void ExpectedToBe(this Token token, TokenDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            if (!token.Is(descriptor))
            {
                throw new CaliParseException($"Unexpected input '{token.Value}' but was expecting '{descriptor.ReportableName}'",
                    token.Line, token.Column);
            }
        }

        internal static bool Is(this Token token, TokenDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            return token.Descriptor == descriptor;
        }

        internal static bool IsNot(this Token token, TokenDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            return token.Descriptor != descriptor;
        }
    }
}