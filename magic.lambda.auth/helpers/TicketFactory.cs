/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace magic.lambda.auth.helpers
{
    /// <summary>
    /// Helper class for creating, retrieving and verifying a JWT token from a ticket.
    /// </summary>
	public static class TicketFactory
	{
        /// <summary>
        /// Creates a JWT token from the specified ticket.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="ticket">Existing user ticket, containing username and roles.</param>
        /// <returns></returns>
        public static string CreateTicket(IConfiguration configuration, Ticket ticket)
        {
            // Getting data to put into token.
            var secret = configuration["auth:secret"] ?? "THIS_IS_NOT_A_GOOD_SECRET";
            var validMinutes = int.Parse(configuration["auth:valid-minutes"] ?? "20");
            var key = Encoding.ASCII.GetBytes(secret);

            // Creating our token descriptor.
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, ticket.Username),
                }),
                Expires = DateTime.UtcNow.AddMinutes(validMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
            };

            // Adding all roles.
            tokenDescriptor.Subject.AddClaims(ticket.Roles.Select(x => new Claim(ClaimTypes.Role, x)));

            // Creating token and returning to caller.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Verifies that the current user belongs to the specified role.
        /// </summary>
        /// <param name="services">Service provider, needed to retrieve the IHttpContextAccessor</param>
        /// <param name="role"></param>
        public static void VerifyTicket(IServiceProvider services, string role)
        {
            // Retrieving the HttpContext object.
            var contextAccessor = services.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var context = contextAccessor.HttpContext;
            if (context == null)
                throw new ApplicationException("No HTTP context exists");

            // Verifying we're dealing with an authenticated user.
            var user = context.User;
            if (user == null || !user.Identity.IsAuthenticated)
                throw new ApplicationException("Access denied");

            // Checking if caller is also trying to check if user belongs to some role(s).
            if (!string.IsNullOrEmpty(role))
            {
                // Verifying the users belongs to (at least) one of the comma separated roles supplied.
                var inRole = false;
                foreach (var idxRole in role.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (user.IsInRole(idxRole.Trim()))
                    {
                        inRole = true;
                        break;
                    }
                }

                // Throwing an exception unless the user belongs to the specified role.
                if (!inRole)
                    throw new ApplicationException("Access denied");
            }
        }

        /// <summary>
        /// Returns the ticket belonging to the specified user.
        /// </summary>
        /// <param name="services">Service provider, necessary to retrieve the IHttpContextAccessor</param>
        /// <returns></returns>
        public static Ticket GetTicket(IServiceProvider services)
        {
            // Retrieving the HttpContext object.
            var contextAccessor = services.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            var context = contextAccessor.HttpContext;
            if (context == null)
                throw new ApplicationException("No HTTP context exists");

            // Verifying we're dealing with an authenticated user.
            var user = context.User;
            if (user == null || !user.Identity.IsAuthenticated)
                throw new ApplicationException("Access denied");

            // Finding roles for user.
            var identity = user.Identity as ClaimsIdentity;
            var username = identity.Claims
                .Where(c => c.Type == ClaimTypes.Name)
                .Select(c => c.Value).First();
            var roles = identity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return new Ticket(username, roles);
        }

    }
}
