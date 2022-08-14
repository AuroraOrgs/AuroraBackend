using Aurora.Shared.Config;
using Aurora.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Aurora.Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection BindConfigSections(this IServiceCollection services, IConfiguration config, params Assembly[] assemblies)
        {
            var serviceExtensionsType = typeof(OptionsServiceCollectionExtensions);
            var attributeType = typeof(ConfigSectionAttribute);
            var binderType = typeof(GenericConfigBinder<>);
            var actionType = typeof(Action<>);
            var allTypes = assemblies.Select(assembly => assembly.GetTypes()).Flatten();
            var types = allTypes.Where(type => Attribute.IsDefined(type, attributeType));

            MethodInfo configureMethod = serviceExtensionsType
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Configure" && x.GetParameters().Length == 2);
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
                    var bindAction = Delegate.CreateDelegate(bindActionType, binder!, "Bind");
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
