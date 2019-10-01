/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.auth.helpers;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.verify-ticket] slot, for verifying that a user is authenticated, and optionally belongs to
    /// one of the roles supplied as a comma separated list of values.
    /// </summary>
	[Slot(Name = "auth.verify-ticket")]
	public class VerifyTicket : ISlot
	{
        readonly IServiceProvider _services;

        /// <summary>
        /// Creates a new instance of class.
        /// </summary>
        /// <param name="services">Service provider, necessary to retrieve the IHttpConextAccessor.</param>
        public VerifyTicket(IServiceProvider services)
		{
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to signal.</param>
        public void Signal(ISignaler signaler, Node input)
		{
            TicketFactory.VerifyTicket(_services, input.GetEx<string>());
            input.Value = true;
		}
    }
}
