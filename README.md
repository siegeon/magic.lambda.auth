
# Magic Lambda Auth

[![Build status](https://travis-ci.org/polterguy/magic.lambda.auth.svg?master)](https://travis-ci.org/polterguy/magic.lambda.auth)

Authentication and authorization helpers for [Magic](https://github.com/polterguy/magic). This project allows you to create and consume
JWT tokens, to secure your magic installation. The project contains 3 slots.

* __[auth.ticket.create]__ - Creates a new JWT token, that you can return to your client, any ways you see fit.
* __[auth.ticket.refresh]__ - Refreshes a JWT token. Useful for refreshing a token before it expires, which would require the user to login again.
* __[auth.ticket.verify]__ - Verifies a JWT token, and that the user is logged in, in addition to (optionally) that he belongs to one roles supplied as a comma separated list of roles.

Notice, you will have to modify your `auth:secret` configuration setting, to provide a unique salt for your installation. If you don't do
this, some adversary can easily reproduce your tokens, and impersonate your users. Example of appSettings.config settings you could
apply (don't use the exact same salt, the idea is to provide a _random_ salt, unique for _your_ installation)

```json
{
	"auth": {
		"secret":"some-rubbish-random23545456-characters-goes-here!!!!!"
	}
}
```

The idea is that your `SymmetricSecretKey` is based upon the above configuration setting, implying you should safe keep it the
same way you'd safe keep the pin code to your ATM card.

## License

Magic is licensed as Affero GPL. This means that you can only use it to create Open Source solutions.
If this is a problem, you can contact at thomas@gaiasoul.com me to negotiate a proprietary license if
you want to use the framework to build closed source code. This will allow you to use Magic in closed
source projects, in addition to giving you access to Microsoft SQL Server adapters, to _"crudify"_
database tables in MS SQL Server. I also provide professional support for clients that buys a
proprietary enabling license.
