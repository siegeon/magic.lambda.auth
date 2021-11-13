﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using magic.lambda.auth.contracts;

namespace magic.lambda.auth.services
{
    /// <summary>
    /// HTTP ticket provider service implementation.
    /// 
    /// Provides a thin layer of abstraction between retrieving authenticated user, 
    /// and the HttpContext to disassociate the HttpContext form the ticket declaring currently logged in user.
    /// </summary>
    public class HttpTicketProvider : ITicketProvider
    {
        readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Creates a new instance of class.
        /// </summary>
        /// <param name="contextAccessor">HTTP context accessor, necessary to retrieve the currently 
        /// authenticated user from the HttpContext.</param>
        public HttpTicketProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc/>
        public string Username => _contextAccessor.HttpContext.User.Identity.Name;

        /// <inheritdoc/>
        public IEnumerable<string> Roles => (_contextAccessor.HttpContext.User.Identity as ClaimsIdentity).Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(x => x.Value);

        /// <inheritdoc/>
        public IEnumerable<(string Name, string Value)> Claims => (_contextAccessor.HttpContext.User.Identity as ClaimsIdentity).Claims
            .Where(c =>
                c.Type != ClaimTypes.Role &&
                c.Type != "nbf" &&
                c.Type != "exp" &&
                c.Type != "iat" &&
                c.Type != ClaimTypes.Name)
            .Select(x => (x.Type, x.Value));

        /// <inheritdoc/>
        public bool InRole(string role)
        {
            return _contextAccessor.HttpContext.User.IsInRole(role?.Trim() ?? throw new ArgumentNullException(nameof(role)));
        }

        /// <inheritdoc/>
        public bool IsAuthenticated()
        {
            return _contextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
