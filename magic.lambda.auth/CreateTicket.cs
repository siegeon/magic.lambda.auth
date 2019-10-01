/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.lambda.auth.helpers;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.auth
{
    // TODO: Consider using YALOA to support the removal of the dependency upon IHttpContextAccessor and HttpContext.
    /// <summary>
    /// [auth.create-ticket] slot for creating a new JWT token.
    /// </summary>
	[Slot(Name = "auth.create-ticket")]
	public class CreateTicket : ISlot
	{
		readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="configuration">Configuration for application.</param>
		public CreateTicket(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

        /// <summary>
        /// Implementation for the slots.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your signal.</param>
		public void Signal(ISignaler signaler, Node input)
		{
            if (input.Value != null)
                throw new ArgumentException($"[auth.create-ticket] don't know how to handle parameters in its value.");

            if (input.Children.Any(x => x.Name != "username" && x.Name != "roles"))
                throw new ApplicationException("[auth.create-ticket] can only handle [username] and [roles] children nodes");

            var usernameNode = input.Children.Where(x => x.Name == "username");
            var rolesNode = input.Children.Where(x => x.Name == "roles");

            if (usernameNode.Count() != 1)
                throw new ApplicationException("[auth.create-ticket] must be given a [username] argument at the minimum");

            var username = usernameNode.First().GetEx<string>();
            var roles = rolesNode.First().Children.Select(x => x.GetEx<string>());

            input.Clear();
            input.Value = TicketFactory.CreateTicket(_configuration, new Ticket(username, roles));
		}
    }
}
