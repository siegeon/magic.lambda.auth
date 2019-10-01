/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using System.Collections.Generic;

namespace magic.lambda.auth.helpers
{
    /// <summary>
    /// Authorization ticket wrapper class, for encapsulating a user and its roles.
    /// </summary>
	public class Ticket
	{
        /// <summary>
        /// Creates a new ticket instance.
        /// </summary>
        /// <param name="username">Username for your ticket.</param>
        /// <param name="roles">roles the user belongs to.</param>
        public Ticket(string username, IEnumerable<string> roles)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Roles = new List<string>(roles ?? new string[] { });
        }

        /// <summary>
        /// Username of the user.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Roles the user belongs to.
        /// </summary>
        public IEnumerable<string> Roles { get; private set; }
    }
}
