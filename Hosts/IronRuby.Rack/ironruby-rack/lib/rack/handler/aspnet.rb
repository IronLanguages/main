require File.dirname(__FILE__) + '/../../../bin/IronRuby.Rack'

module Rack
  
  module Handler
  
    # See Src\AspNet.cs for the implementation
    class ASPNET < IronRubyRack::Handler::AspNet
    end
  
  end
  
end