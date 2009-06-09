Merb::Router.extensions do
  # Use this method in your router to ensure that the user is authenticated
  #
  # This will run through any strategies that you have setup, or that you declare
  # as an argument to the authenticate method.
  #
  # ===Example
  #  
  #    authenticate(OpenID) do
  #       resource :posts
  #      
  #       authenticate do
  #         match("/").to(:controller => "home")
  #       end
  #    end
  # 
  # This is a simple example that shows protecting the entire set of routes for
  # the posts resource with the OpenID strategy.  
  #
  # The match on "/" is protected, _first_ by the OpenID strategy,
  # then by the dfeeault set of stratgies.  Strategies are applied from the
  # outer block first, working to the inner blocks.
  def authenticate(*strategies, &block)
    p = Proc.new do |request, params|
      if request.session.authenticated?
        params
      else
        if strategies.blank?
          result = request.session.authenticate!(request, params)
        else
          result = request.session.authenticate!(request, params, *strategies)
        end
      
        if request.session.authentication.halted?
          auth = request.session.authentication
          [auth.status, auth.headers, auth.body]
        else
          params
        end
      end
    end
    defer(p, &block)
  end
  
end