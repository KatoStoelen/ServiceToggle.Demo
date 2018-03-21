using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ServiceToggle.Demo.Services;
using ServiceToggle.Demo.Toggle;
using ServiceToggle.Windsor.Extensions;

namespace ServiceToggle.Demo.Extensions
{
    public static class WindsorContainerExtensions
    {
        public static IWindsorContainer RegisterServices(this IWindsorContainer @this)
        {
            return
                @this.Register(
                    Component
                        .For<IValueService>()
                        .ImplementedBy<OriginalValueService>()
                        .LifestyleTransient()
                );
        }

        public static IWindsorContainer RegisterToggles(this IWindsorContainer @this, LaunchDarklyClient client)
        {
            @this.UseNamingSubSystemWithToggleSupport();

            return
                @this.Register(
                    Component
                        .For<IValueService>()
                        .ImplementedBy<NewValueService>()

                        .Replace(typeof(IValueService), typeof(OriginalValueService))

                        .ResolvableIf(() => client.IsFeatureEnabled("new-value-feature"))
                );
        }
    }
}