using System.Collections.Generic;
using System.Reflection;
using Autofac;
using AutoMapper;
using Module = Autofac.Module;

namespace UBeat.Crm.CoreApi.Services
{
    public class AutoMapperModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(AutoMapperModule).GetTypeInfo().Assembly;
            //register all profile classes in the calling assembly
            builder.RegisterAssemblyTypes(assembly).AssignableTo<Profile>().As<Profile>();

            builder.Register(context => new MapperConfiguration(cfg =>
            {
                //cfg.CreateMissingTypeMaps = false;
                //cfg.ShouldMapField = fieldInfo => false;
                //cfg.AllowNullCollections = true;
                //cfg.DisableConstructorMapping();
                foreach (var profile in context.Resolve<IEnumerable<Profile>>())
                {
                    cfg.AddProfile(profile);
                }
            })).AsSelf().SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper(c.Resolve))
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }
    }
}
