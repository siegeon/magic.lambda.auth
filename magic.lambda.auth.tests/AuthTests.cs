/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using magic.node;
using magic.node.extensions;
using magic.lambda.exceptions;

namespace magic.lambda.auth.tests
{
    public class AuthTests
    {
        [Fact]
        public void AuthenticateNoRoles()
        {
            var signaler = Common.Initialize();
            var args = new Node();
            args.Add(new Node("username", "foo"));
            signaler.Signal("auth.ticket.create", args);
            Assert.NotNull(args.Value);
            Assert.True(args.Get<string>().Length > 20);
        }

        [Fact]
        public void AuthenticateNoRoles_Throws()
        {
            var signaler = Common.Initialize();
            var args = new Node();
            Assert.Throws<ArgumentException>(() => signaler.Signal("auth.ticket.create", args));
        }

        [Fact]
        public void RefreshTicket()
        {
            var signaler = Common.Initialize();
            var args = new Node();
            args.Add(new Node("username", "foo"));
            signaler.Signal("auth.ticket.create", args);
            var nArgs = new Node();
            signaler.Signal("auth.ticket.refresh", nArgs);
            Assert.NotEqual(args.Value, nArgs.Value);
            Assert.True(nArgs.Get<string>().Length > 20);
        }

        [Fact]
        public void ParseToken()
        {
            var signaler = Common.Initialize();
            var args = new Node();
            args.Add(new Node("username", "foo"));
            args.Add(new Node("roles", null, new Node[] { new Node("", "howdy1") }));
            signaler.Signal("auth.ticket.create", args);
            var token = new JwtSecurityToken(jwtEncodedString: args.Get<string>());
            Assert.Equal("foo", token.Payload["unique_name"]);
            Assert.Equal("howdy1", token.Payload["role"]);
        }

        [Fact]
        public void VerifyTicket()
        {
            var signaler = Common.Initialize();
            signaler.Signal("auth.ticket.verify", new Node()); // Notice, our TicketProvder in Common.cs will sort this out for us.
        }

        [Fact]
        public void VerifyTicketNoConfig()
        {
            var signaler = Common.Initialize(true, false);
            signaler.Signal("auth.ticket.verify", new Node()); // Notice, our TicketProvder in Common.cs will sort this out for us.
        }

        [Fact]
        public void InRoleNoConfig()
        {
            var signaler = Common.Initialize(true, false);
            var node = new Node("", "bar1");
            signaler.Signal("auth.ticket.in-role", node); // Notice, our TicketProvder in Common.cs will sort this out for us.
            Assert.True(node.Get<bool>());
        }

        [Fact]
        public void NotInRoleNoConfig()
        {
            var signaler = Common.Initialize(true, false);
            var node = new Node("", "bar3");
            signaler.Signal("auth.ticket.in-role", node); // Notice, our TicketProvder in Common.cs will sort this out for us.
            Assert.False(node.Get<bool>());
        }

        [Fact]
        public void VerifyTicket_Throws()
        {
            var signaler = Common.Initialize(false);
            Assert.Throws<HyperlambdaException>(() => signaler.Signal("auth.ticket.verify", new Node()));
        }

        [Fact]
        public void GetTicket()
        {
            var signaler = Common.Initialize();
            var node = new Node();
            signaler.Signal("auth.ticket.get", node); // Notice, our TicketProvder in Common.cs will sort this out for us.
            Assert.Equal("foo", node.Value);
            Assert.Equal(2, node.Children.FirstOrDefault(x => x.Name == "roles")?.Children.Count() ?? -1);
            Assert.Equal("bar1", node.Children.FirstOrDefault(x => x.Name == "roles")?.Children.First().Value);
            Assert.Equal("bar2", node.Children.FirstOrDefault(x => x.Name == "roles")?.Children.Skip(1).First().Value);
        }

        [Fact]
        public void VerifyRole_01()
        {
            var signaler = Common.Initialize();
            signaler.Signal("auth.ticket.verify", new Node("", "bar1")); // Notice, our TicketProvder in Common.cs will sort this out for us.
        }

        [Fact]
        public void VerifyRole_02()
        {
            var signaler = Common.Initialize();
            signaler.Signal("auth.ticket.verify", new Node("", "bar2")); // Notice, our TicketProvder in Common.cs will sort this out for us.
        }

        [Fact]
        public void VerifyRole_Throws()
        {
            var signaler = Common.Initialize();
            Assert.Throws<HyperlambdaException>(() => signaler.Signal("auth.ticket.verify", new Node("", "bar2-XXX")));
        }
    }
}
