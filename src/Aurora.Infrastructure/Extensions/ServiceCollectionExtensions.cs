using Aurora.Infrastructure.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Aurora.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection BindConfigSections(this IServiceCollection services, IConfiguration config)
        {
            var serviceExtensionsType = typeof(OptionsServiceCollectionExtensions);
            var attributeType = typeof(ConfigSectionAttribute);
            var binderType = typeof(GenericConfigBinder<>);
            var actionType = typeof(Action<>);
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => Attribute.IsDefined(type, attributeType));

            var configureMethod = serviceExtensionsType.GetMethods(BindingFlags.Static | BindingFlags.Public).SingleOrDefault(x => x.Name == "Configure" && x.GetParameters().Length == 2);
            foreach (var type in types)
            {
                var currentBinderType = binderType.MakeGenericType(type);
                var currentConfigureMethod = configureMethod.MakeGenericMethod(type);
                var attribute = Attribute.GetCustomAttribute(type, attributeType);
                if (attribute is not null && attribute is ConfigSectionAttribute sectionAttribute)
                {
                    var sectionNames = sectionAttribute.SectionNames;
                    var section = sectionNames.Aggregate(config, (currentConfig, sectionName) => currentConfig.GetSection(sectionName));
                    var binder = Activator.CreateInstance(currentBinderType, section);
                    var bindActionType = actionType.MakeGenericType(type);
                    var bindAction = Delegate.CreateDelegate(bindActionType, binder, "Bind");
                    currentConfigureMethod.Invoke(null, new object[] { services, bindAction });
                }
            }
            return services;
        }

        private class GenericConfigBinder<T>
        {
            private readonly IConfiguration _configSection;

            public GenericConfigBinder(IConfiguration configSection)
            {
                _configSection = configSection;
            }

            public void Bind(T item)
            {
                _configSection.Bind(item);
            }
        }
    }
}
