require Merb.framework_root / "merb-core" / "dispatch" / "default_exception" / "default_exception"

module Merb
  class Dispatcher
    class << self
      include Merb::ControllerExceptions
      
      # :api: private
      attr_accessor :use_mutex
      
      @@work_queue = Queue.new
      
      # ==== Returns
      # Queue:: the current queue of dispatch jobs.
      # 
      # :api: private
      def work_queue
        @@work_queue
      end  
      
      Merb::Dispatcher.use_mutex = ::Merb::Config[:use_mutex]
      
      # Dispatch the rack environment. ControllerExceptions are rescued here
      # and redispatched.
      #
      # ==== Parameters
      # rack_env<Rack::Environment>::
      #   The rack environment, which is used to instantiate a Merb::Request
      #
      # ==== Returns
      # Merb::Controller::
      #   The Merb::Controller that was dispatched to
      # 
      # :api: private
      def handle(request)
        request.handle
      end
    end
  end
  
  class Request
    include Merb::ControllerExceptions
    
    @@mutex = Mutex.new
    
    # Handles request routing and action dispatch.
    # 
    # ==== Returns
    # Merb::Controller:: the controller that handled the action dispatch.
    # 
    # :api: private
    def handle
      start = Time.now
      Merb.logger.info { "Started request handling: #{start.to_s}" }
      
      find_route!
      return rack_response if handled?
      
      klass = controller
      Merb.logger.debug { "Routed to: #{params.inspect}" }
      
      unless klass < Controller
        raise NotFound, 
          "Controller '#{klass}' not found.\n" \
          "If Merb tries to find a controller for static files, " \
          "you may need to check your Rackup file, see the Problems " \
          "section at: http://wiki.merbivore.com/pages/rack-middleware"
      end
      
      if klass.abstract?
        raise NotFound, "The '#{klass}' controller has no public actions"
      end
      
      controller = dispatch_action(klass, params[:action])
      controller._benchmarks[:dispatch_time] = Time.now - start
      Merb.logger.info { controller._benchmarks.inspect }
      Merb.logger.flush
      controller.rack_response
    rescue Object => exception
      dispatch_exception(exception).rack_response
    end
    
    private
    # Setup the controller and call the chosen action 
    #
    # ==== Parameters
    # klass<Merb::Controller>:: The controller class to dispatch to.
    # action<Symbol>:: The action to dispatch.
    # status<Integer>:: The status code to respond with.
    #
    # ==== Returns
    # Merb::Controller::
    #   The Merb::Controller that was dispatched to.
    # 
    # :api: private
    def dispatch_action(klass, action, status=200)
      # build controller
      controller = klass.new(self, status)
      if Dispatcher.use_mutex
        @@mutex.synchronize { controller._dispatch(action) }
      else
        controller._dispatch(action)
      end
      controller
    end
    
    # Re-route the current request to the Exception controller if it is
    # available, and try to render the exception nicely.  
    #
    # You can handle exceptions by implementing actions for specific
    # exceptions such as not_found or for entire classes of exceptions
    # such as client_error. You can also implement handlers for 
    # exceptions outside the Merb exception hierarchy (e.g.
    # StandardError is caught in standard_error).
    #
    # ==== Parameters
    # exception<Object>::
    #   The exception object that was created when trying to dispatch the
    #   original controller.
    #
    # ==== Returns
    # Exceptions::
    #   The Merb::Controller that was dispatched to. 
    # 
    # :api: private
    def dispatch_exception(exception)
      if(exception.is_a?(Merb::ControllerExceptions::Base) &&
         !exception.is_a?(Merb::ControllerExceptions::ServerError))
        Merb.logger.info(Merb.exception(exception))
      else
        Merb.logger.error(Merb.exception(exception))
      end
      
      self.exceptions = [exception]
      
      begin
        e = exceptions.first
        
        if action_name = e.action_name
          dispatch_action(Exceptions, action_name, e.class.status)
        else
          Merb::Dispatcher::DefaultException.new(self, e.class.status)._dispatch
        end
      rescue Object => dispatch_issue
        if e.same?(dispatch_issue) || exceptions.size > 5
          Merb::Dispatcher::DefaultException.new(self, e.class.status)._dispatch
        else
          Merb.logger.error("Dispatching #{e.class} raised another error.")
          Merb.logger.error(Merb.exception(dispatch_issue))
          
          exceptions.unshift dispatch_issue
          retry
        end
      end
    end
  end
end
