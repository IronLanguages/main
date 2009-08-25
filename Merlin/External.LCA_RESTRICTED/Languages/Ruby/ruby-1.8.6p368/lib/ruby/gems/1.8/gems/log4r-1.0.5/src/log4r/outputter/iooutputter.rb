# :nodoc:
require "log4r/outputter/outputter"
require "log4r/staticlogger"

module Log4r
  
  ##
  # IO Outputter invokes print then flush on the wrapped IO
  # object. If the IO stream dies, IOOutputter sets itself to OFF
  # and the system continues on its merry way.
  #
  # To find out why an IO stream died, create a logger named 'log4r'
  # and look at the output.

  class IOOutputter < Outputter

    # IOOutputter needs an IO object to write to.
    def initialize(_name, _out, hash={})
      super(_name, hash)
      @out = _out
    end

    def closed?
      @out.closed?
    end

    # Close the IO and sets level to OFF
    def close
      @out.close unless @out.nil?
      @level = OFF
      OutputterFactory.create_methods(self)
      Logger.log_internal {"Outputter '#{@name}' closed IO and set to OFF"}
    end

    #######
    private
    #######
    
    # perform the write
    def write(data)
      begin
        @out.print data
        @out.flush
      rescue IOError => ioe # recover from this instead of crash
        Logger.log_internal {"IOError in Outputter '#{@name}'!"}
        Logger.log_internal {ioe}
        close
      rescue NameError => ne
        Logger.log_internal {"Outputter '#{@name}' IO is #{@out.class}!"}
        Logger.log_internal {ne}
        close
      end
    end
  end
end
