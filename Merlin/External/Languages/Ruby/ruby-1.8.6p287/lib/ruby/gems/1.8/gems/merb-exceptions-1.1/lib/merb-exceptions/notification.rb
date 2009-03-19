require 'net/http'
require 'uri'
require 'erb'
require 'merb-mailer'

module MerbExceptions
  class Notification

    class Mailer < Merb::Mailer
    end

    attr_reader :details

    def initialize(details = nil)
      @details = details || []
      Mailer.config = Merb::Plugins.config[:exceptions][:mailer_config]
      Mailer.delivery_method = Merb::Plugins.config[:exceptions][:mailer_delivery_method]
    end

    def deliver!
      deliver_web_hooks!
      deliver_emails!
    end

    def deliver_web_hooks!
      Merb.logger.info "DELIVERING EXCEPTION WEB HOOKS"
      web_hooks.each do |address|
        post_hook(address)
      end
    end

    def deliver_emails!
      Merb.logger.info  "DELIVERING EXCEPTION EMAILS"
      email_addresses.each do |address|
        send_email(address)
      end
    end

    def web_hooks; option_as_array(:web_hooks); end

    def email_addresses; option_as_array(:email_addresses); end

    def environments; option_as_array(:environments); end

    def params
      @params ||=
      {
        'request_url'              => details['url'],
        'request_controller'       => details['params'][:controller],
        'request_action'           => details['params'][:action],
        'request_params'           => details['params'],
        'environment'              => details['environment'],
        'exceptions'               => details['exceptions'],
        'app_name'                 => Merb::Plugins.config[:exceptions][:app_name]
      }
    end

  private

    def post_hook(address)
      Merb.logger.info "- hooking to #{address}"
      uri = URI.parse(address)
      uri.path = '/' if uri.path=='' # set a path if one isn't provided to keep Net::HTTP happy
      Net::HTTP.post_form( uri, params ).body
    end

    def email_body
      @body ||= begin
        path = File.join(File.dirname(__FILE__), 'templates', 'email.erb')
        template = Erubis::Eruby.new(File.open(path,'r') { |f| f.read })
        template.result(binding)
      end
    end

    def send_email(address)
      Merb.logger.info "- emailing to #{address}"
      email = Mailer.new({
        :to => address,
        :from => Merb::Plugins.config[:exceptions][:email_from],
        :subject => "[#{Merb::Plugins.config[:exceptions][:app_name]} EXCEPTION]",
        :text => email_body
      })
      email.deliver!
    end

    # Used so that we can accept either a single value or array (e.g. of
    # webhooks) in our YAML file.
    def option_as_array(option)
      value = Merb::Plugins.config[:exceptions][option]
      case value
      when Array
        value.reject { |e| e.nil? } # Don't accept nil values
      when String
        [value]
      else
        []
      end
    end
  end
end
