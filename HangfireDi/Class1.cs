using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
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

    public interface IDependency
    {
        void Go();
    }

    public class ADummyJob
    {
        private readonly IDependency _dependency;

        public ADummyJob(IDependency dependency)
        {
            _dependency = dependency;
        }

        public void Go()
        {
            _dependency.Go();
        }
    }

    public class ConcreteDependency : IDependency
    {
        public void Go()
        {
            Debug.WriteLine("Hi there");
        }
    }

    public class ContainerJobActivator : JobActivator
    {
        readonly IServiceProvider _container;

        public ContainerJobActivator(IServiceProvider serviceProvider)
        {
            _container = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            object o = _container.GetService(type);

            Debug.WriteLine(o.GetType());

            return o;
        }
    }

    public class ScopedContainerJobActivator : JobActivator
    {
        readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedContainerJobActivator(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ServiceJobActivatorScope(_serviceScopeFactory.CreateScope());
        }

        private class ServiceJobActivatorScope : JobActivatorScope
        {
            readonly IServiceScope _serviceScope;

            public ServiceJobActivatorScope(IServiceScope serviceScope)
            {
                if (serviceScope == null)
                {
                    throw new ArgumentNullException(nameof(serviceScope));
                }

                _serviceScope = serviceScope;
            }

            public override object Resolve(Type type)
            {
                return _serviceScope.ServiceProvider.GetRequiredService(type);
            }
        }
    }

    public class ApiDependencyResolver : ApiDependencyScope, System.Web.Http.Dependencies.IDependencyResolver
    {
        public ApiDependencyResolver(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public System.Web.Http.Dependencies.IDependencyScope BeginScope()
        {
            return new ApiDependencyScope(ServiceProvider.CreateScope());
        }
    }

    public class MvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public MvcDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                if (!(HttpContext.Current.Items[typeof(MvcDependencyResolver)] is IServiceScope scope))
                {
                    HttpContext.Current.Items[typeof(MvcDependencyResolver)] = scope = _serviceProvider.CreateScope();
                }

                return scope.ServiceProvider;
            }
        }

        public static void DisposeServiceScope()
        {
            if (HttpContext.Current.Items[typeof(MvcDependencyResolver)] is IServiceScope scope)
            {
                scope.Dispose();
            }
        }
    }

    public class ApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
    {
        protected readonly IServiceProvider ServiceProvider;

        private readonly IServiceScope _serviceScope;

        public ApiDependencyScope(IServiceScope serviceScope)
            : this(serviceScope.ServiceProvider)
        {
            _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
        }

        protected ApiDependencyScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }
    }

}