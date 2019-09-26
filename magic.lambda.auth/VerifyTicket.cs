/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;
using magic.node;
using magic.node.extensions;
using magic.lambda.auth.init;
using magic.signals.contracts;

namespace magic.lambda.auth
{
	[Slot(Name = "auth.verify-ticket")]
	public class VerifyTicket : ISlot
	{
        readonly IServiceProvider _services;
        readonly HttpService _httpService;

        public VerifyTicket(
            IServiceProvider services,
            HttpService httpService)
		{
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
        }

        public void Signal(ISignaler signaler, Node input)
		{
            _httpService.VerifyTicket(_services, input.GetEx<string>());
            input.Value = true;
		}
    }
}
