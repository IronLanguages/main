module Merb
  module MailerMixin
    
    # Sends mail via a MailController (a tutorial can be found in the
    # MailController docs).
    #
    # ==== Parameters
    # klass<Class>:: The mailer class.
    # method<~to_s>:: The method to call on the mailer.
    # mail_params<Hash>::
    #   Mailing parameters, e.g. :to and :cc. See
    #   Merb::MailController#dispatch_and_deliver for details.
    # send_params<Hash>::
    #   Params to send to the mailer. Defaults to the params of the current
    #   controller.
    #
    # ==== Examples
    #   # Send an email via the FooMailer's bar method.
    #   send_mail FooMailer, :bar, :from => "foo@bar.com", :to => "baz@bat.com"
    def send_mail(klass, method, mail_params, send_params = nil)
      klass.new(send_params || params, self).dispatch_and_deliver(method, mail_params)
    end
      
  end
end