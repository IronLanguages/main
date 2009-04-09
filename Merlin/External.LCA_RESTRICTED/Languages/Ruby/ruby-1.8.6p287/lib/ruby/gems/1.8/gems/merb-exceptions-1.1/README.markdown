merb-exceptions
===============
A simple Merb plugin to ease exception notifications.

The notifier currently supports two interfaces, Email Alerts and Web Hooks. Emails are formatted as plain text and sent using your Merb environment's mail settings. Web hooks as sent as post requests.

Getting Going
-------------
Once you have the Gem installed you will need to add it as a dependency in your projects `dependencies.rb` file

    dependency 'merb-exceptions'

Configuration goes in your projects `config/init.rb` file inside `Merb::BootLoader.before_app_loads`. See the 'Settings' section below for a full description of the options.

    Merb::Plugins.config[:exceptions] = {
      :web_hooks       => ['http://example.com'],
      :email_addresses => ['hello@exceptions.com', 'user@myapp.com'],
      :app_name        => "My App Name",
      :environments    => ['production', 'staging'],
      :email_from      => "exceptions@myapp.com",
      :mailer_config => nil,
      :mailer_delivery_method => :sendmail
    }

The plugin now automatically includes itself into your Exceptions controller. If you are using an old version of this plugin, you can remove the include from your Exceptions controller.

If you have specified any email addresses, and are not already requiring merb-mailer, then you need to do so. It also needs configuration.

    dependency 'merb-mailer'

Settings
--------
`web_hooks`, `email_addresses`, and `environments` can either be a single string or an array of strings.

`app_name`: Used to customise emails and web hooks (default "My App")

`email_from`: Exceptions are sent from this address

`web_hooks`: Each url is sent a post request. See 'Web Hooks' for more info.

`email_addresses`: Each email address is sent an exception notification using Merb's built in mailer settings.

`environments`: Notifications will only be sent for environments in this list, defaults to `production`

`mailer_delivery_method`: The delivery method for notifications mailer, see merb-mailer documentation.

`mailer_config`: A hash of configuration options for the notifications mailer, see merb-mailer documentation.

Advanced usage
--------------
merb-exceptions will deliver exceptions for any unhandled exceptions (exceptions that do not have views defined in the `Exceptions` controller)

You can cause handled exceptions to send notifications as well, for example to be notified of 404's:

    after :notify_of_exceptions, :only => :not_found

    def not_found
      render_and_notify :format => :html
    end

`notify_of_exceptions` - sends notifications without any rendering logic. Note though that if you are sending lots of notifications this could delay sending a response back to the user so it is better to use after rendering.

Web hooks
---------
Web hooks are a great way to push your data beyond your app to the outside world. For each address on your `web_hooks` list we will send a HTTP:POST request with the following parameters for you to consume.

WEBHOOKS FORMATTING IS CURRENTLY BROKEN. WILL POST AN EXAMPLE OF THE CORRECT FORMAT HERE WHEN IT'S FIXED.


Licence
-------
(The MIT License)

Copyright (c) 2008 New Bamboo

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
'Software'), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.