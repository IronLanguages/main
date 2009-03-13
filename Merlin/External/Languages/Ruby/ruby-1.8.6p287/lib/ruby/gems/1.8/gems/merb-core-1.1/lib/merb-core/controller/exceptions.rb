class Exception
  # Returns the action_name that will be invoked on your Exceptions controller when this
  # exception is raised. Override this method to force a different action to be invoked.
  #
  # ==== Returns
  # String:: The name of the action in the Exceptions controller which will get invoked
  #   when this exception is raised during a request.
  # 
  # :api: public
  # @overridable
  def action_name() self.class.action_name end
  
  
  # ==== Returns
  # Boolean:: Whether or not this exception is the same as another.
  #
  # :api: public
  def same?(other)
    self.class == other.class &&
    self.message == other.message &&
    self.backtrace == other.backtrace
  end
  
  # Returns the action_name that will be invoked on your Exceptions controller when an instance
  # is raised during a request.
  #
  # ==== Returns
  # String:: The name of the action in the Exceptions controller which will get invoked
  #   when an instance of this Exception sub/class is raised by an action.
  # 
  # :api: public
  # @overridable
  def self.action_name
    if self == Exception
      return nil unless Object.const_defined?(:Exceptions) && 
        Exceptions.method_defined?(:exception)
    end
    name = self.to_s.split('::').last.snake_case
    Object.const_defined?(:Exceptions) && 
      Exceptions.method_defined?(name) ? name : superclass.action_name
  end
  
  # The status that will be sent in the response when an instance is
  # raised during a request. Override this to send a different status.
  #
  # ==== Returns
  # Integer:: The status code to send in the response. Defaults to 500.
  #
  # :api: public
  # @overridable
  def self.status
    500
  end
end

module Merb
  # ControllerExceptions are a way of simplifying controller code by placing
  # exception logic back into the MVC pattern.
  #
  # When a ControllerException is raised within your application merb will
  # attempt to re-route the request to your Exceptions controller to render
  # the error in a friendly manor.
  #
  # For example you might have an action in your app that raises NotFound
  # if a resource was not available
  #

  #   def show
  #     product = Product.find(params[:id])
  #     raise NotFound if product.nil?
  #     [...]
  #   end
  #
  # This would halt execution of your action and re-route it over to your
  # Exceptions controller which might look something like:
  #
  # class Exceptions < Merb::Controller

  #   def not_found
  #     render :layout => :none
  #   end
  # end
  #
  # As usual, the not_found action will look for a template in
  #   app/views/exceptions/not_found.html.erb
  #
  # Note: All standard ControllerExceptions have an HTTP status code associated 
  # with them which is sent to the browser when the action is rendered.
  #
  # Note: If you do not specifiy how to handle raised ControllerExceptions 
  # or an unhandlable exception occurs within your customised exception action
  # then they will be rendered using the built-in error template.
  # In development mode this "built in" template will show stack-traces for
  # any of the ServerError family of exceptions (you can force the stack-trace
  # to display in production mode using the :exception_details config option in 
  # merb.yml)
  #
  #
  # ==== Internal Exceptions 
  #
  # Any other rogue errors (not ControllerExceptions) that occur during the 
  # execution of your app will be converted into the ControllerException 
  # InternalServerError. And like all other exceptions, the ControllerExceptions  
  # can be caught on your Exceptions controller.
  #
  # InternalServerErrors return status 500, a common use for customizing this
  # action might be to send emails to the development team, warning that their
  # application has exploded. Mock example:
  #

  #   def internal_server_error
  #     MySpecialMailer.deliver(
  #       "team@cowboys.com", 
  #       "Exception occured at #{Time.now}", 
  #       self.request.exceptions.first)
  #     render 'Something is wrong, but the team is on it!'
  #   end
  #
  # Note: The special method +exceptions+ is available on Merb::Request instances 
  # and contains the exceptions that was raised (this is handy if
  # you want to display the associated message or display more detailed info).
  #
  #
  # ==== Extending ControllerExceptions
  #
  # To extend the use of the ControllerExceptions one may extend any of the 
  # HTTPError classes.
  #
  # As an example we can create an exception called AdminAccessRequired.
  #
  #   class AdminAccessRequired < Merb::ControllerExceptions::Unauthorized; end
  #
  # Add the required action to our Exceptions controller
  #
  #   class Exceptions < Merb::Controller

  #     def admin_access_required
  #       render
  #     end
  #   end
  #
  # In app/views/exceptions/admin_access_required.rhtml
  # 
  #   <h1>You're not an administrator!</h1>
  #   <p>You tried to access <%= @tried_to_access %> but that URL is 
  #   restricted to administrators.</p>
  #
  module ControllerExceptions
    
    # Mapping of status code names to their numeric value.
    STATUS_CODES = {}

    class Base < StandardError #:doc:

      # === Returns
      # Integer:: The status-code of the error.
      # 
      # @overridable
      # :api: plugin
      def status; self.class.status; end
      alias :to_i :status

      class << self

        # Get the actual status-code for an Exception class.
        #
        # As usual, this can come from a constant upwards in
        # the inheritance chain.
        #
        # ==== Returns
        # Fixnum:: The status code of this exception.
        #
        # :api: public
        def status
          const_get(:STATUS) rescue 0
        end
        alias :to_i :status
        
        # Set the actual status-code for an Exception class.
        #
        # If possible, set the STATUS constant, and update
        # any previously registered (inherited) status-code.
        #
        # ==== Parameters
        # num<~to_i>:: The status code
        #
        # ==== Returns
        # (Integer, nil):: The status set on this exception, or nil if a status was already set.
        #
        # :api: private
        def status=(num)
          unless self.status?
            register_status_code(self, num)
            self.const_set(:STATUS, num.to_i)
          end
        end
      
        # See if a status-code has been defined (on self explicitly).
        #
        # ==== Returns
        # Boolean:: Whether a status code has been set
        #
        # :api: private
        def status?
          self.const_defined?(:STATUS)
        end
      
        # Registers any subclasses with status codes for easy lookup by
        # set_status in Merb::Controller.
        #
        # Inheritance ensures this method gets inherited by any subclasses, so
        # it goes all the way down the chain of inheritance.
        #
        # ==== Parameters
        # 
        # subclass<Merb::ControllerExceptions::Base>::
        #   The Exception class that is inheriting from Merb::ControllerExceptions::Base
        #
        # :api: public
        def inherited(subclass)
          # don't set the constant yet - any class methods will be called after self.inherited
          # unless self.status = ... is set explicitly, the status code will be inherited
          register_status_code(subclass, self.status) if self.status?
        end
        
        private
        
        # Register the status-code for an Exception class.
        #
        # ==== Parameters
        # num<~to_i>:: The status code
        #
        # :api: privaate
        def register_status_code(klass, code)
          name = self.to_s.split('::').last.snake_case
          STATUS_CODES[name.to_sym] = code.to_i
        end
        
      end
    end

    class Informational                 < Merb::ControllerExceptions::Base; end

      class Continue                    < Merb::ControllerExceptions::Informational; self.status = 100; end

      class SwitchingProtocols          < Merb::ControllerExceptions::Informational; self.status = 101; end

    class Successful                    < Merb::ControllerExceptions::Base; end

      class OK                          < Merb::ControllerExceptions::Successful; self.status = 200; end

      class Created                     < Merb::ControllerExceptions::Successful; self.status = 201; end

      class Accepted                    < Merb::ControllerExceptions::Successful; self.status = 202; end

      class NonAuthoritativeInformation < Merb::ControllerExceptions::Successful; self.status = 203; end

      class NoContent                   < Merb::ControllerExceptions::Successful; self.status = 204; end

      class ResetContent                < Merb::ControllerExceptions::Successful; self.status = 205; end

      class PartialContent              < Merb::ControllerExceptions::Successful; self.status = 206; end

    class Redirection                   < Merb::ControllerExceptions::Base; end

      class MultipleChoices             < Merb::ControllerExceptions::Redirection; self.status = 300; end

      class MovedPermanently            < Merb::ControllerExceptions::Redirection; self.status = 301; end

      class MovedTemporarily            < Merb::ControllerExceptions::Redirection; self.status = 302; end

      class SeeOther                    < Merb::ControllerExceptions::Redirection; self.status = 303; end

      class NotModified                 < Merb::ControllerExceptions::Redirection; self.status = 304; end

      class UseProxy                    < Merb::ControllerExceptions::Redirection; self.status = 305; end

      class TemporaryRedirect           < Merb::ControllerExceptions::Redirection; self.status = 307; end

    class ClientError                   < Merb::ControllerExceptions::Base; end

      class BadRequest                  < Merb::ControllerExceptions::ClientError; self.status = 400; end

      class MultiPartParseError         < Merb::ControllerExceptions::BadRequest; end

      class Unauthorized                < Merb::ControllerExceptions::ClientError; self.status = 401; end

      class PaymentRequired             < Merb::ControllerExceptions::ClientError; self.status = 402; end

      class Forbidden                   < Merb::ControllerExceptions::ClientError; self.status = 403; end

      class NotFound                    < Merb::ControllerExceptions::ClientError; self.status = 404; end

      class ActionNotFound              < Merb::ControllerExceptions::NotFound; end

      class TemplateNotFound            < Merb::ControllerExceptions::NotFound; end

      class LayoutNotFound              < Merb::ControllerExceptions::NotFound; end

      class MethodNotAllowed            < Merb::ControllerExceptions::ClientError; self.status = 405; end

      class NotAcceptable               < Merb::ControllerExceptions::ClientError; self.status = 406; end

      class ProxyAuthenticationRequired < Merb::ControllerExceptions::ClientError; self.status = 407; end

      class RequestTimeout              < Merb::ControllerExceptions::ClientError; self.status = 408; end

      class Conflict                    < Merb::ControllerExceptions::ClientError; self.status = 409; end

      class Gone                        < Merb::ControllerExceptions::ClientError; self.status = 410; end

      class LengthRequired              < Merb::ControllerExceptions::ClientError; self.status = 411; end

      class PreconditionFailed          < Merb::ControllerExceptions::ClientError; self.status = 412; end

      class RequestEntityTooLarge       < Merb::ControllerExceptions::ClientError; self.status = 413; end

      class RequestURITooLarge          < Merb::ControllerExceptions::ClientError; self.status = 414; end

      class UnsupportedMediaType        < Merb::ControllerExceptions::ClientError; self.status = 415; end

      class RequestRangeNotSatisfiable  < Merb::ControllerExceptions::ClientError; self.status = 416; end

      class ExpectationFailed           < Merb::ControllerExceptions::ClientError; self.status = 417; end

    class ServerError                   < Merb::ControllerExceptions::Base; end

      class InternalServerError         < Merb::ControllerExceptions::ServerError; self.status = 500; end

      class NotImplemented              < Merb::ControllerExceptions::ServerError; self.status = 501; end

      class BadGateway                  < Merb::ControllerExceptions::ServerError; self.status = 502; end

      class ServiceUnavailable          < Merb::ControllerExceptions::ServerError; self.status = 503; end

      class GatewayTimeout              < Merb::ControllerExceptions::ServerError; self.status = 504; end

      class HTTPVersionNotSupported     < Merb::ControllerExceptions::ServerError; self.status = 505; end
  end
  
  # Required to show exceptions in the log file
  #
  # e<Exception>:: The exception that a message is being generated for
  #
  # :api: plugin
  def self.exception(e)
    "#{ e.message } - (#{ e.class })\n" <<  
    "#{(e.backtrace or []).join("\n")}" 
  end

end
