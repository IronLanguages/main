# :nodoc:
module Log4r

  ##
  # LogEvent wraps up all the miscellaneous data associated with a logging
  # statement. It gets passed around to the varied components of Log4r and
  # should be of interest to those creating extensions.
  #
  # Data contained: 
  #
  # [level]    The integer level of the log event. Use LNAMES[level]
  #            to get the actual level name.
  # [tracer]   The execution stack returned by <tt>caller</tt> at the
  #            log event. It is nil if the invoked Logger's trace is false.
  # [data]     The object that was passed into the logging method.
  # [name]     The name of the logger that was invoked.
  # [fullname] The fully qualified name of the logger that was invoked.
  #
  # Note that creating timestamps is a task left to formatters.

  class LogEvent
    attr_reader :level, :tracer, :data, :name, :fullname
    def initialize(level, logger, tracer, data)
      @level, @tracer, @data = level, tracer, data
      @name, @fullname = logger.name, logger.fullname
    end
  end
end
