using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireDi
{
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
}