using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// A comparer compare string in case-insensitive mode,
    /// which only take effects on standard latin letters.
    /// </summary>
    public class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        /// <inheritdoc />
        public bool Equals(string? x, string? y)
        {
            ReadOnlySpan<char> s1 = x, s2 = y;

            if (s1.Length != s2.Length)
            {
                return false;
            }

            for (int i = 0; i < s1.Length; i++)
            {
                char c1 = s1[i];
                char c2 = s2[i];

                if (c1 >= 'A' && c1 <= 'Z')
                {
                    c1 = (char)(c1 + 32);
                }
                if (c2 >= 'A' && c2 <= 'Z')
                {
                    c2 = (char)(c2 + 32);
                }

                if (c1 != c2)
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] string obj)
        {
            char[] chars = obj.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= 'A' && chars[i] <= 'Z')
                {
                    chars[i] = (char)(chars[i] + 32);
                }
            }
            return new string(chars).GetHashCode();
        }
    }
}
