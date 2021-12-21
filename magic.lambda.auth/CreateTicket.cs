/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using magic.node;
using magic.node.contracts;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.auth.helpers;

namespace magic.lambda.auth
{
    /// <summary>
    /// [auth.ticket.create] slot for creating a new JWT token.
    /// </summary>
    [Slot(Name = "auth.ticket.create")]
    public class CreateTicket : ISlot
    {
        readonly IMagicConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for application.</param>
        public CreateTicket(IMagicConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Implementation for the slots.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your signal.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var username = input.Children.FirstOrDefault(x => x.Name == "username")?.GetEx<string>()
                ?? throw new HyperlambdaException("No [username] supplied to [auth.ticket.create]");
            var roles = input.Children
                .FirstOrDefault(x => x.Name == "roles")?
                .Children
                .Select(x => x.GetEx<string>())
                .ToArray();
            var claims = input.Children
                .FirstOrDefault(x => x.Name == "claims")?
                .Children
                .Select(x => (x.Name, x.GetEx<string>()))
                .ToArray();
            var expires = input.Children.FirstOrDefault(x => x.Name == "expires")?.GetEx<DateTime?>();

            input.Clear();
            input.Value = TicketFactory.CreateTicket(
                _configuration,
                new Ticket(username, roles, claims, expires));
        }
    }
}
