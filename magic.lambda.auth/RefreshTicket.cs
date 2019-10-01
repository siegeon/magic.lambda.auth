/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.signals.contracts;
using magic.lambda.auth.helpers;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.refresh-ticket] slot refreshing an existing ticket, resulting in a new ticket,
    /// with a postponed expiration time, to avoid having users having to login every time their
    /// token expires.
    /// </summary>
	[Slot(Name = "auth.refresh-ticket")]
	public class RefreshTicket : ISlot
	{
        readonly IServiceProvider _services;
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        /// <param name="services">Service provider, necessary to retrieve the IHttpContextAccessor.</param>
        public RefreshTicket(IConfiguration configuration, IServiceProvider services)
		{
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Implementation for your slot
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
		{
            // This will throw is ticket is expired, doesn't exist, etc.
            TicketFactory.VerifyTicket(_services, null);

            // Retrieving old ticket and using its data to create a new ticket.
            input.Value = TicketFactory.CreateTicket(_configuration, TicketFactory.GetTicket(_services));
		}
    }
}
