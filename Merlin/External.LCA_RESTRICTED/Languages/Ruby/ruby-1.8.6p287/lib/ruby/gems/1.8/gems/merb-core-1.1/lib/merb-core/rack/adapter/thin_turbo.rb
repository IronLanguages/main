require "thin-turbo"

module Merb

  module Rack

    class ThinTurbo < Thin
      # start a Thin Turbo server on given host and port.

      # :api: plugin
      def self.new_server(port)
        @opts.merge!(:backend => ::Thin::Backends::Turbo)
        super
      end

    end
  end
end
