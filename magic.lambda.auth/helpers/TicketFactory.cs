﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using magic.node.contracts;
using magic.node.extensions;
using magic.lambda.auth.contracts;

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
        /// <returns>A JWT token</returns>
        public static string CreateTicket(
            IMagicConfiguration configuration,
            Ticket ticket)
        {
            // Getting data to put into token.
            var secret = configuration["magic:auth:secret"] ??
                throw new SecurityException("We couldn't find any 'magic:auth:secret' setting in your applications configuration");
            var key = Encoding.UTF8.GetBytes(secret);

            // Creating our token descriptor.
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, ticket.Username),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            // Setting expiration date of ticket.
            if (ticket.Expires.HasValue)
            {
                // Notice, if expiration date is min value of DateTime we assume caller wants a token that "never expires", hence setting it 5 years into the future!
                if (ticket.Expires.Value != DateTime.MinValue)
                    tokenDescriptor.Expires = ticket.Expires.Value;
                else
                    tokenDescriptor.Expires = DateTime.Now.AddYears(5);

            }
            else
            {
                // Using default expiration value from configuration for JWT ticket.
                var validMinutes = int.Parse(configuration["magic:auth:valid-minutes"] ?? "20");
                tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(validMinutes);
            }

            // Adding all roles.
            tokenDescriptor.Subject.AddClaims(ticket.Roles.Select(x => new Claim(ClaimTypes.Role, x)));

            // Adding all additional claims.
            tokenDescriptor.Subject.AddClaims(ticket.Claims.Select(x => new Claim(x.Name, x.Value)));

            // Creating token and returning to caller.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Verifies that the current user belongs to the specified role.
        /// </summary>
        /// <param name="ticketProvider">Service provider, needed to retrieve the IHttpContextAccessor</param>
        /// <param name="roles"></param>
        public static void VerifyTicket(ITicketProvider ticketProvider, string roles)
        {
            if (!ticketProvider.IsAuthenticated())
                throw new HyperlambdaException("Access denied", true, 401);

            if (!string.IsNullOrEmpty(roles))
            {
                if (!roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(x => ticketProvider.InRole(x)))
                    throw new HyperlambdaException("Access denied", true, 401);
            }
        }

        /// <summary>
        /// Returns true if user belongs to any of the specified role(s) supplied as
        /// a comma separated list of values.
        /// </summary>
        /// <param name="ticketProvider">Service provider, needed to retrieve the IHttpContextAccessor</param>
        /// <param name="roles"></param>
        public static bool InRole(ITicketProvider ticketProvider, string roles)
        {
            if (!ticketProvider.IsAuthenticated())
                return false;

            if (string.IsNullOrEmpty(roles))
                return false;
            return roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(x => ticketProvider.InRole(x));
        }

        /// <summary>
        /// Returns the ticket belonging to the specified user.
        /// </summary>
        /// <param name="ticketProvider">Service provider, necessary to retrieve the IHttpContextAccessor</param>
        /// <returns></returns>
        public static Ticket GetTicket(ITicketProvider ticketProvider)
        {
            return new Ticket(ticketProvider.Username, ticketProvider.Roles, ticketProvider.Claims);
        }
    }
}
