/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
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
            readonly bool _auth;
            public TicketProvider(bool auth)
            {
                _auth = auth;
            }

            public string Username => _auth ? "foo" : null;

            public IEnumerable<string> Roles => _auth ? new string[] { "bar1", "bar2" } : new string[] { };

            public bool InRole(string role)
            {
                return Roles.Contains(role);
            }

            public bool IsAuthenticated()
            {
                return _auth;
            }
        }

        public static ISignaler Initialize(bool createTicket = true, bool config = true)
        {
            var services = new ServiceCollection();
            services.AddTransient<ISignaler, Signaler>();
            var types = new SignalsProvider(InstantiateAllTypes<ISlot>(services));
            services.AddTransient<ISignalsProvider>((svc) => types);
            services.AddTransient<ITicketProvider, TicketProvider>((svc) => new TicketProvider(createTicket));
            var mockConfiguration = new Mock<IConfiguration>();
            if (config)
            {
                mockConfiguration.SetupGet(x => x[It.Is<string>(x2 => x2 == "magic:auth:secret")]).Returns("some-secret-goes-here");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x2 => x2 == "magic:auth:valid-minutes")]).Returns("20");
            }
            services.AddTransient((svc) => mockConfiguration.Object);
            var provider = services.BuildServiceProvider();
            return provider.GetService<ISignaler>();
        }

        #region [ -- Private helper methods -- ]

        static IEnumerable<Type> InstantiateAllTypes<T>(ServiceCollection services) where T : class
        {
            var type = typeof(T);
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !x.FullName.StartsWith("Microsoft", StringComparison.InvariantCulture))
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
