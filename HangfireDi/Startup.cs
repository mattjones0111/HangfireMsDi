using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Mvc;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HangfireDi.Startup))]

namespace HangfireDi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            IServiceCollection services = GetServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            MvcDependencyResolver resolver = new MvcDependencyResolver(serviceProvider);
            DependencyResolver.SetResolver(resolver);

            HttpConfiguration config = new HttpConfiguration();
            config.DependencyResolver = new ApiDependencyResolver(serviceProvider);

            HangfireAspNet.Use(() => GetHangfireServers(serviceProvider));

            BackgroundJob.Enqueue(() => Debug.WriteLine("Hello world from Hangfire!"));

            BackgroundJob.Enqueue<ADummyJob>(x => x.Go());

            app.UseWebApi(config);
        }

        static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddTransient<IDependency, ConcreteDependency>();
            services.AddTransient<ADummyJob>();

            return services;
        }

        IEnumerable<IDisposable> GetHangfireServers(IServiceProvider serviceProvider)
        {
            Hangfire.GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage()
                .UseActivator(new ScopedContainerJobActivator(serviceProvider));

            yield return new BackgroundJobServer();
        }
    }
}
