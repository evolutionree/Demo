using System.Collections.Generic;
using Autofac;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class CoreApiServiceLocator
    {
        private readonly IComponentContext _container;

        public CoreApiServiceLocator(IComponentContext container)
        {
            _container = container;
        }

        public TService GetInstance<TService>()
        {
            return _container.Resolve<TService>();
        }

        public TService GetInstance<TService>(IEnumerable<NamedParameter> parameters)
        {
            return _container.Resolve<TService>(parameters);
        }
    }
}
