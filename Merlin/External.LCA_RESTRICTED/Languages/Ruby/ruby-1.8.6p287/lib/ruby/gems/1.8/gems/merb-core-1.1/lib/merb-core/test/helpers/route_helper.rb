module Merb
  module Test
    module RouteHelper
      include RequestHelper
      
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
      def url(*args)
        args << (@request_params || {})
        Merb::Router.url(*args)
      end
      
      # Mimics the resource method available to controllers
      #
      # ==== Paramaters
      # resources<Object>:: The resources to generate URLs from
      # params<Hash>:: Any extra parameters that are required.
      #
      # ==== Returns
      # String:: The generated URL.
      def resource(*args)
        args << @request_params || {}
        Merb::Router.resource(*args)
      end
      
      # ==== Parameters
      # path<~to_string>:: The URL of the request.
      # method<~to_sym>:: HTTP request method.
      # env<Hash>:: Additional parameters for the request.
      #
      # ==== Returns
      # Hash:: A hash containing the controller and action along with any parameters
      def request_to(path, method = :get, env = {})
        env[:request_method] ||= method.to_s.upcase
        env[:request_uri] = path
        
        check_request_for_route(build_request({}, env))
      end
    end
  end
end
