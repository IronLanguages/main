require 'swiftcore/evented_mongrel'
require 'merb-core/rack/handler/mongrel'
module Merb
  module Rack

    class EventedMongrel < Mongrel
      # :api: plugin
      def self.new_server(port)
        Merb::Dispatcher.use_mutex = false
        super
      end
    end
  end
end
