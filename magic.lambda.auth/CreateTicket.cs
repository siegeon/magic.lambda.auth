/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using magic.node;
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
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for application.</param>
        public CreateTicket(IConfiguration configuration)
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
                ?? throw new ArgumentException("No [username] supplied to [auth.ticket.create]");
            var roles = input.Children
                .FirstOrDefault(x => x.Name == "roles")?
                .Children
                .Select(x => x.GetEx<string>())
                .ToArray();
            var claims = input.Children
                .Where(x => x.Name != "roles" && x.Name != "username")
                .Select(x => (x.Name, Converter.ToString(x.Value).Item2))
                .ToList();

            input.Clear();
            input.Value = TicketFactory.CreateTicket(
                _configuration,
                new Ticket(username, roles, claims));
        }
    }
}
