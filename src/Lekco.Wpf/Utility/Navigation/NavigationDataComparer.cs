using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lekco.Wpf.Utility.Navigation
{
    public class NavigationDataComparer : IEqualityComparer<NavigationData>
    {
        public bool Equals(NavigationData? x, NavigationData? y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.Key.Equals(y.Key);
        }

        public int GetHashCode([DisallowNull] NavigationData obj)
        {
            return obj.Key?.GetHashCode() ?? 0;
        }
    }
}
