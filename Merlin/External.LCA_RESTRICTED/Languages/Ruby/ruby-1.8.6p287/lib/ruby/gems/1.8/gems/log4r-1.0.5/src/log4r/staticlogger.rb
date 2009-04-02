# :nodoc:
module Log4r
  class Logger
    # Returns the root logger. Identical to Logger.global
    def self.root; return RootLogger.instance end
    # Returns the root logger. Identical to Logger.root
    def self.global; return root end
    
    # Get a logger with a fullname from the repository or nil if logger
    # wasn't found.

    def self.[](_fullname)
      # forces creation of RootLogger if it doesn't exist yet.
      return RootLogger.instance if _fullname=='root' or _fullname=='global'
      Repository[_fullname]
    end

    # Like Logger[] except that it raises NameError if Logger wasn't found.

    def self.get(_fullname)
      logger = self[_fullname]
      if logger.nil?
        raise NameError, "Logger '#{_fullname}' not found.", caller
      end
      logger
    end

    # Yields fullname and logger for every logger in the system.
    def self.each
      for fullname, logger in Repository.instance.loggers
        yield fullname, logger
      end
    end

    def self.each_logger
      Repository.instance.loggers.each_value {|logger| yield logger}
    end

    # Internal logging for Log4r components. Accepts only blocks.
    # To see such log events, create a logger named 'log4r' and give 
    # it an outputter.

    def self.log_internal(level=1)
      internal = Logger['log4r']
      return if internal.nil?
      internal.send(LNAMES[level].downcase, yield)
    end
  end
end
