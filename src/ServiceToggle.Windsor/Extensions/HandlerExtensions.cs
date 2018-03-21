using Castle.Core.Internal;
using Castle.MicroKernel;
using System;

namespace ServiceToggle.Windsor.Extensions
{
    internal static class HandlerExtensions
    {
        public static Func<bool> GetIsActivePredicate(this IHandler @this)
        {
            var predicateProperty = @this.ComponentModel.ExtendedProperties[PropertyKey.Predicate];

            if (predicateProperty != null && predicateProperty is Func<bool> predicate)
                return predicate;

            return null;
        }

        public static Replacement GetReplacement(this IHandler @this)
        {
            var replacementProperty = @this.ComponentModel.ExtendedProperties[PropertyKey.Replacement];

            if (replacementProperty != null && replacementProperty is Replacement replacement)
                return replacement;

            return null;
        }

        public static Func<Type, int> GetServicePriorityFunction(this IHandler @this)
        {
            var defaultsFilter = @this.ComponentModel.GetDefaultComponentForServiceFilter();
            var fallbackFilter = @this.ComponentModel.GetFallbackComponentForServiceFilter();

            return serviceType =>
            {
                if (defaultsFilter != null && defaultsFilter.Invoke(serviceType))
                    return 1;
                if (fallbackFilter != null && fallbackFilter.Invoke(serviceType))
                    return -1;

                return 0;
            };
        }
    }
}
