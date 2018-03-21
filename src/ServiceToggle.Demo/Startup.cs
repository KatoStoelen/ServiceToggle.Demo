using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceToggle.Demo.Configuration;
using ServiceToggle.Demo.Toggle;
using ServiceToggle.Demo.Extensions;

namespace ServiceToggle.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LaunchDarklyClient = new LaunchDarklyClient(configuration.Get<DemoOptions>().LaunchDarklyOptions);
        }

        private IConfiguration Configuration { get; }
        private LaunchDarklyClient LaunchDarklyClient { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddCors();
            
            var windsorContainer = new WindsorContainer()
                .RegisterServices()
                .RegisterToggles(LaunchDarklyClient);

            return WindsorRegistrationHelper.CreateServiceProvider(windsorContainer, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            app.UseCors(builder =>
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

            app.UseMvc();
        }
    }
}
