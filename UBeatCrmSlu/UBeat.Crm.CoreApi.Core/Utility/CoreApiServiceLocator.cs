using System.Collections.Generic;
using Autofac;
using System.Reflection;
using System;

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
        public object GetInstanceWithName(string servicename) {

            System.Type t = null;
            try
            {
                var assembly = Assembly.Load(new AssemblyName("UBeat.Crm.CoreApi.Services"));
                t= assembly.GetType(servicename);
            }
            catch (Exception ex) {
            }
            //ConstructorInfo[] acts = t.GetConstructors();
            //ConstructorInfo defaultAct = acts[0];
            //ParameterInfo[] paramsInfo = defaultAct.GetParameters();
            //foreach (ParameterInfo p in paramsInfo) {
            //    p.GetType()
            //}
            return _container.Resolve(t);
        }

        public TService GetInstance<TService>(IEnumerable<NamedParameter> parameters)
        {
            return _container.Resolve<TService>(parameters);
        }
    }
}
