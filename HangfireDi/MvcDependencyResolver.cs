using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireDi
{
    public class MvcDependencyResolver : IDependencyResolver
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
}