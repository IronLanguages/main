# :include: rdoc/logserver

require 'log4r/logger'
require 'log4r/lib/drbloader'

module Log4r
  # See log4r/logserver.rb
  class LogServer < Logger
    attr_reader :uri

    # A valid ROMP uri must be specified.
    def initialize(_fullname, _uri, _level=nil, 
                   _additive=true, _trace=false, &accept)
      super(_fullname, _level, _additive, _trace)
      @uri = _uri
      start_server(_uri, accept)
      Logger.log_internal {"LogServer started at #{@uri}"}
    end

    if HAVE_ROMP
      include ROMPServer
    else
      def initialize(*args)
        raise RuntimeError, "LogServer not supported. ROMP is required", caller
      end
    end
  end
end
