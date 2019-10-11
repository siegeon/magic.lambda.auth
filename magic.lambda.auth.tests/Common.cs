/*
 * Magic, Copyright(c) Thomas Hansen 2019, thomas@gaiasoul.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using magic.signals.services;
using magic.signals.contracts;
using magic.lambda.auth.contracts;

namespace magic.lambda.auth.tests
{
    public static class Common
    {
        private class TicketProvider : ITicketProvider
        {
            public string Username => "foo";

            public IEnumerable<string> Roles => new string[] { "bar1", "bar2" };

            public bool InRole(string role)
            {
                return Roles.Contains(role);
            }

            public bool IsAuthenticated()
            {
                return true;
            }
        }

        public static ISignaler Initialize()
        {
            var services = new ServiceCollection();
            services.AddTransient<ISignaler, Signaler>();
            var types = new SignalsProvider(InstantiateAllTypes<ISlot>(services));
            services.AddTransient<ISignalsProvider>((svc) => types);
            services.AddTransient<ITicketProvider, TicketProvider>();
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[It.Is<string>(x2 => x2 == "magic:auth:secret")]).Returns("some-secret-goes-here");
            mockConfiguration.SetupGet(x => x[It.Is<string>(x2 => x2 == "magic:auth:valid-minutes")]).Returns("20");
            services.AddTransient((svc) => mockConfiguration.Object);
            var provider = services.BuildServiceProvider();
            return provider.GetService<ISignaler>();
        }

        #region [ -- Private helper methods -- ]

        static IEnumerable<Type> InstantiateAllTypes<T>(ServiceCollection services) where T : class
        {
            var type = typeof(T);
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var idx in result)
            {
                services.AddTransient(idx);
            }
            return result;
        }

        #endregion
    }
}
