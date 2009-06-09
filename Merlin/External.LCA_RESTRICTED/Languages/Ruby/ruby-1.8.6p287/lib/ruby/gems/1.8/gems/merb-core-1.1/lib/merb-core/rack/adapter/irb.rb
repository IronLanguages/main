module Merb
  module Rack
    class Console
      # There are three possible ways to use this method.  First, if you have a named route, 
      # you can specify the route as the first parameter as a symbol and any paramters in a 
      # hash.  Second, you can generate the default route by just passing the params hash, 
      # just passing the params hash.  Finally, you can use the anonymous parameters.  This 
      # allows you to specify the parameters to a named route in the order they appear in the 
      # router.  
      #
      # ==== Parameters(Named Route)
      # name<Symbol>:: 
      #   The name of the route. 
      # args<Hash>:: 
      #   Parameters for the route generation.
      #
      # ==== Parameters(Default Route)
      # args<Hash>:: 
      #   Parameters for the route generation.  This route will use the default route. 
      #
      # ==== Parameters(Anonymous Parameters)
      # name<Symbol>::
      #   The name of the route.  
      # args<Array>:: 
      #   An array of anonymous parameters to generate the route
      #   with. These parameters are assigned to the route parameters
      #   in the order that they are passed.
      #
      # ==== Returns
      # String:: The generated URL.
      #
      # ==== Examples
      # Named Route
      #
      # Merb::Router.prepare do
      #   match("/articles/:title").to(:controller => :articles, :action => :show).name("articles")
      # end
      #
      # url(:articles, :title => "new_article")
      #
      # Default Route
      #
      # Merb::Router.prepare do
      #   default_routes
      # end
      #
      # url(:controller => "articles", :action => "new")
      #
      # Anonymous Paramters
      #
      # Merb::Router.prepare do
      #   match("/articles/:year/:month/:title").to(:controller => :articles, :action => :show).name("articles")
      # end
      #
      # url(:articles, 2008, 10, "test_article")
      #
      # :api: public
      def url(name, *args)
        args << {}
        Merb::Router.url(name, *args)
      end

      # Reloads classes using Merb::BootLoader::ReloadClasses.
      # :api: public
      def reload!
        Merb::BootLoader::ReloadClasses.reload
      end

      # Prints all routes for the application.
      # :api: public
      def show_routes
        seen = []
        unless Merb::Router.named_routes.empty?
          puts "==== Named routes"
          Merb::Router.named_routes.each do |name,route|
            # something weird happens when you combine sprintf and irb
            puts "Helper     : #{name}"
            meth = $1.upcase if route.conditions[:method].to_s =~ /(get|post|put|delete)/
            puts "HTTP method: #{meth || 'GET'}"
            puts "Route      : #{route}"
            puts "Params     : #{route.params.inspect}"
            puts
            seen << route
          end
        end
        puts "==== Anonymous routes"
        (Merb::Router.routes - seen).each do |route|
          meth = $1.upcase if route.conditions[:method].to_s =~ /(get|post|put|delete)/
          puts "HTTP method: #{meth || 'GET'}"
          puts "Route      : #{route}"
          puts "Params     : #{route.params.inspect}"
          puts
        end
        nil
      end

      # Starts a sandboxed session (delegates to any Merb::Orms::* modules).
      #
      # An ORM should implement Merb::Orms::MyOrm#open_sandbox! to support this.
      # Usually this involves starting a transaction.
      # :api: public
      def open_sandbox!
        puts "Loading #{Merb.environment} environment in sandbox (Merb #{Merb::VERSION})"
        puts "Any modifications you make will be rolled back on exit"
        orm_modules.each { |orm| orm.open_sandbox! if orm.respond_to?(:open_sandbox!) }
      end

      # Ends a sandboxed session (delegates to any Merb::Orms::* modules).
      #
      # An ORM should implement Merb::Orms::MyOrm#close_sandbox! to support this.
      # Usually this involves rolling back a transaction.
      # :api: public
      def close_sandbox!
        orm_modules.each { |orm| orm.close_sandbox! if orm.respond_to?(:close_sandbox!) }
        puts "Modifications have been rolled back"
      end

      # Explictly show logger output during IRB session
      # :api: public
      def trace_log!
        Merb.logger.auto_flush = true
      end

      private

      # ==== Returns
      # Array:: All Merb::Orms::* modules.
      # :api: private
      def orm_modules
        if Merb.const_defined?('Orms')
          Merb::Orms.constants.map { |c| Merb::Orms::const_get(c) }
        else
          []
        end
      end

    end

    class Irb
      # ==== Parameters
      # opts<Hash>:
      #   Options for IRB. Currently this is not used by the IRB adapter.
      #
      # ==== Notes
      # If the +.irbrc+ file exists, it will be loaded into the IRBRC
      # environment variable.
      #
      # :api: plugin
      def self.start(opts={})
        m = Merb::Rack::Console.new
        m.extend Merb::Test::RequestHelper
        m.extend ::Webrat::Methods if defined?(::Webrat)
        Object.send(:define_method, :merb) { m }
        ARGV.clear # Avoid passing args to IRB
        m.open_sandbox! if sandboxed?
        require 'irb'
        require 'irb/completion'
        if File.exists? ".irbrc"
          ENV['IRBRC'] = ".irbrc"
        end
        IRB.start
        at_exit do merb.close_sandbox! if sandboxed? end
        exit
      end

      private

      # :api: private
      def self.sandboxed?
        Merb::Config[:sandbox]
      end
    end
  end
end
