require "merb-mailer/mailer"
require "merb-mailer/mail_controller"
require "merb-mailer/mailer_mixin"

Merb::Controller.send(:include, Merb::MailerMixin)
Merb.add_generators(File.join(File.dirname(__FILE__), 'generators', 'mailer_generator'))