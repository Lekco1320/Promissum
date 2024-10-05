using System.Collections.Concurrent;

namespace Lekco.Wpf.Utility.Navigation
{
    public static class NavigationServiceManager
    {
        private static readonly ConcurrentDictionary<object, INavigationService> _mapping;

        static NavigationServiceManager()
        {
            _mapping = new ConcurrentDictionary<object, INavigationService>();
        }

        public static bool Register(object key, INavigationService navigationService)
        {
            return _mapping.TryAdd(key, navigationService);
        }

        public static void UnregisterService(object key)
        {
            _mapping.TryRemove(key, out var _);
        }

        public static INavigationService GetService(object key)
        {
            return _mapping[key];
        }
    }
}
