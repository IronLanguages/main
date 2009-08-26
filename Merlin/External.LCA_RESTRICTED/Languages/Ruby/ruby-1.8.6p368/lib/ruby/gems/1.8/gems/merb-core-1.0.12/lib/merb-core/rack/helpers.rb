module Merb
  module Rack
    module Helpers
      
      # A helper to build a rack response which implements a redirect.  The status will be set to 
      # the passed in status if passed.  If you pass in permanent it will be a 301, permanent redirect,
      # otherwise it defaults to a temporary 302 redirect.  
      #
      # ==== Parameters
      # url<~to_s>:: The url to redirect to.
      # options<Hash>:: A hash of options for the redirect
      #   status: The status code to use for the redirect
      #   permanent:  True if this is a permanent redirect (301)
      #
      # ==== Returns
      # <Array>:: A rack response to redirect to the specified url.  
      #
      # :api: plugin
      def self.redirect(url, options = {})
        # Build the rack array
        status   = options.delete(:status)
        status ||= options[:permanent] ? 301 : 302
        
        Merb.logger.info("Dispatcher redirecting to: #{url} (#{status})")
        Merb.logger.flush
        
        [status, { Merb::Const::LOCATION => url },
         Merb::Rack::StreamWrapper.new("<html><body>You are being <a href=\"#{url}\">redirected</a>.</body></html>")]
      end
      
    end
  end
end
