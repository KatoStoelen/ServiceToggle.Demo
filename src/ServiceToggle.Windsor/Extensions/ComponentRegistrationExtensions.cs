using Castle.MicroKernel.Registration;
using System;

namespace ServiceToggle.Windsor.Extensions
{
    public static class ComponentRegistrationExtensions
    {
        public static ComponentRegistration<TService> ResolvableIf<TService>(
            this ComponentRegistration<TService> @this, Func<bool> predicate) where TService : class
        {
            return @this.ExtendedProperties(new Property(PropertyKey.Predicate, predicate));
        }

        public static ComponentRegistration<TService> Replace<TService>(
            this ComponentRegistration<TService> @this, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { typeof(TService) }, implementationTypes);
        }

        public static ComponentRegistration<TService> Replace<TService, TService2>(
            this ComponentRegistration<TService> @this, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { typeof(TService), typeof(TService2) }, implementationTypes);
        }

        public static ComponentRegistration<TService> Replace<TService, TService2, TService3>(
            this ComponentRegistration<TService> @this, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { typeof(TService), typeof(TService2), typeof(TService3) }, implementationTypes);
        }

        public static ComponentRegistration<TService> Replace<TService, TService2, TService3, TService4>(
            this ComponentRegistration<TService> @this, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { typeof(TService), typeof(TService2), typeof(TService3), typeof(TService4) }, implementationTypes);
        }


        public static ComponentRegistration<TService> Replace<TService, TService2, TService3, TService4, TService5>(
            this ComponentRegistration<TService> @this, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { typeof(TService), typeof(TService2), typeof(TService3), typeof(TService4), typeof(TService5) }, implementationTypes);
        }

        public static ComponentRegistration<TService> Replace<TService>(
            this ComponentRegistration<TService> @this, Type serviceType, params Type[] implementationTypes)
            where TService : class
        {
            return @this.Replace(new[] { serviceType }, implementationTypes);
        }

        public static ComponentRegistration<TService> Replace<TService>(
            this ComponentRegistration<TService> @this, Type[] serviceTypes, params Type[] implementationTypes)
            where TService : class
        {
            foreach (var implementationType in implementationTypes)
            {
                foreach (var serviceType in serviceTypes)
                {
                    if (!serviceType.IsAssignableFrom(implementationType))
                        throw new ArgumentException(
                            $"Implementation {implementationType.FullName} does not implement {serviceType.FullName}");
                }
            }

            return @this.ExtendedProperties(new Property(PropertyKey.Replacement, new Replacement
            {
                Services = serviceTypes,
                Implementations = implementationTypes
            }));
        }

        public static ComponentRegistration<TService> Replace<TService>(
            this ComponentRegistration<TService> @this, string componentName)
            where TService : class
        {
            return @this.ExtendedProperties(new Property(PropertyKey.Replacement, new Replacement
            {
                Name = componentName
            }));
        }
    }
}
