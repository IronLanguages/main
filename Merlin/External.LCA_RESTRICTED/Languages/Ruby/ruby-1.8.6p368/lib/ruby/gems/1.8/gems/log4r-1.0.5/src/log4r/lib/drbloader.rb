#:nodoc:
module Log4r
  begin 
    require 'romp'
    HAVE_ROMP = true
  rescue LoadError
    HAVE_ROMP = false
  end
  
  if HAVE_ROMP

    module ROMPServer #:nodoc:
      private
      def start_server(_uri, accept)
        @server = ROMP::Server.new(_uri, accept) # what if accept is nil?
        @server.bind(self, "Log4r::LogServer")
      end
    end

    module ROMPClient #:nodoc:
      private
      def connect
        begin
          @client = ROMP::Client.new(@uri, false)
          @remote_logger = @client.resolve("Log4r::LogServer")
        rescue Exception => e
          Logger.log_internal(-2) {
            "RemoteOutputter '#{@name}' failed to connect to #{@uri}!"
          }
          Logger.log_internal {e}
          self.level = OFF
        end
      end
      # we use propagated = true
      def send_buffer
        begin
          @buff.each {|levent| 
            lname = LNAMES[levent.level].downcase
            @remote_logger.oneway(lname, levent, true)
          }
        rescue Exception => e
          Logger.log_internal(-2) {"RemoteOutputter '#{@name}' can't log!"}
          Logger.log_internal {e}
          self.level = OFF
        ensure @buff.clear
        end
      end
    end

  end

end
