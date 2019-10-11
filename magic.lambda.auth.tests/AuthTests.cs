/*
 * Magic, Copyright(c) Thomas Hansen 2019, thomas@gaiasoul.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Security;
using Xunit;
using magic.node;
using magic.node.extensions;
using System.IdentityModel.Tokens.Jwt;

namespace magic.lambda.auth.tests
{
    public class AuthTests
    {
        [Fact]
        public void Authenticate()
        {
            var signaler = Common.Initialize();
            var args = new Node();
            args.Add(new Node("username", "foo"));
            signaler.Signal("auth.ticket.create", args);
            Assert.NotNull(args.Value);
            Assert.True(args.Get<string>().Length > 20);
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
            Assert.Throws<SecurityException>(() => signaler.Signal("auth.ticket.verify", new Node("", "bar2-XXX")));
        }
    }
}
