using System;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireDi
{
    public class ApiDependencyResolver : ApiDependencyScope, IDependencyResolver
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
}