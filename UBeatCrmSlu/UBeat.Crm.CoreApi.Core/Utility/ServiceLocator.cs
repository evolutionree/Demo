namespace UBeat.Crm.CoreApi.Core.Utility
{
    public static class ServiceLocator
    {
        private static CoreApiServiceLocator _currentProvider;

        public static CoreApiServiceLocator Current => _currentProvider;

        public static void SetLocatorProvider(CoreApiServiceLocator serviceProvider)
        {
            _currentProvider = serviceProvider;
        }
    }
}
