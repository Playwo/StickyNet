using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StickyNet.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class Extensions
    {
        public static IServiceCollection AddStickyServices(this IServiceCollection services)
        {
            foreach (var type in GetStickyServiceTypes())
            {
                object service = Activator.CreateInstance(type);
                services.AddSingleton(type, service);
            }

            return services;
        }

        public static async Task<IServiceCollection> InitializeStickyServicesAsync(this IServiceCollection services)
        {
            foreach (var type in GetStickyServiceTypes())
            {
                var service = services.Where(x => x.ServiceType == type).First().ImplementationInstance as StickyService;

                var fields = type.GetFields().Where(x => x.GetCustomAttribute(typeof(InjectAttribute)) != null);

                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    object fieldService = services.Where(x => x.ServiceType == fieldType).First().ImplementationInstance;
                    field.SetValue(service, fieldService);
                }

                await service.InitializeAsync();
            }

            return services;
        }

        private static IEnumerable<Type> GetStickyServiceTypes()
            => Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(x => x.BaseType == typeof(StickyService) && !x.IsAbstract);

    }
}
