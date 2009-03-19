# :nodoc:
require 'log4r/lib/drbloader'
require 'log4r/outputter/outputter'

module Log4r
  # See log4r/logserver.rb
  class RemoteOutputter < Outputter

    def initialize(_name, hash={})
      super(_name, hash)
      @uri = (hash[:uri] or hash['uri'])
      @buffsize = (hash[:buffsize] or hash['buffsize'] or 1).to_i
      @buff = []
      connect
    end
    
    if HAVE_ROMP
      include ROMPClient
    else
      def initialize(*args)
        raise RuntimeError, "LogServer not supported. ROMP is required", caller
      end
    end


    # Call flush to send any remaining LogEvents to the remote server.
    def flush
      synch { send_buffer }
    end

    private

    def canonical_log(logevent)
      synch {
        @buff.push logevent
        send_buffer if @buff.size >= @buffsize
      }
    end
  end
end
