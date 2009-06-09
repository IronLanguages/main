# make sure we're running inside Merb
if defined?(Merb::Plugins)

  Merb::BootLoader.before_app_loads do

  end

  Merb::BootLoader.after_app_loads do

    # Default configuration
    Merb::Plugins.config[:exceptions] = {
      :web_hooks       => [],
      :email_addresses => [],
      :app_name        => "Merb awesome Application",
      :environments    => ['production'],
      :email_from      => "exceptions@myapp.com",
      :mailer_config => nil,
      :mailer_delivery_method => :sendmail
    }.merge(Merb::Plugins.config[:exceptions] || {})

    if Object.const_defined?(:Exceptions)
      Exceptions.send(:include, MerbExceptions::ExceptionsHelper)
    end
    if Merb::Plugins.config[:exceptions][:environments].include?(Merb.env)
      Merb::Dispatcher::DefaultException.send(:include, MerbExceptions::ExceptionsHelper)
      Merb::Dispatcher::DefaultException.send(:include, MerbExceptions::DefaultExceptionExtensions)
    end
  end

  require 'merb-exceptions/notification'
  require 'merb-exceptions/default_exception_extensions'
  require 'merb-exceptions/exceptions_helper'
end
