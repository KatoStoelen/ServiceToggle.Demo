using Castle.MicroKernel;
using Castle.MicroKernel.SubSystems.Naming;
using Castle.Windsor;

namespace ServiceToggle.Windsor.Extensions
{
    public static class WindsorContainerExtensions
    {
        public static void UseNamingSubSystemWithToggleSupport(this IWindsorContainer @this)
        {
            var namingSubSystem = @this.Kernel.GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem;
            if (namingSubSystem is NamingSubSystemWithToggleSupport)
                return;

            @this.Kernel.AddSubSystem(SubSystemConstants.NamingKey, new NamingSubSystemWithToggleSupport(namingSubSystem));
        }
    }
}
