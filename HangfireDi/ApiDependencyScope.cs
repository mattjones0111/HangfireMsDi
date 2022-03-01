using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireDi
{
    public class ApiDependencyScope : IDependencyScope
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