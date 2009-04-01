require "time" # httpdate
# ==== Public DataObjects Logger API
#
# Logger taken from Merb :)
#
# To replace an existing logger with a new one:
#  DataObjects::Logger.set_log(log{String, IO},level{Symbol, String})
#
# Available logging levels are
#   DataObjects::Logger::{ Fatal, Error, Warn, Info, Debug }
#
# Logging via:
#   DataObjects.logger.fatal(message<String>)
#   DataObjects.logger.error(message<String>)
#   DataObjects.logger.warn(message<String>)
#   DataObjects.logger.info(message<String>)
#   DataObjects.logger.debug(message<String>)
#
# Flush the buffer to
#   DataObjects.logger.flush
#
# Remove the current log object
#   DataObjects.logger.close
#
# ==== Private DataObjects Logger API
#
# To initialize the logger you create a new object, proxies to set_log.
#   DataObjects::Logger.new(log{String, IO},level{Symbol, String})
#
# Logger will not create the file until something is actually logged
# This avoids file creation on DataObjects init when it creates the
# default logger.
module DataObjects

  class << self #:nodoc:
    attr_accessor :logger
  end

  class Logger

    attr_accessor :aio
    attr_accessor :delimiter
    attr_reader   :level
    attr_reader   :buffer
    attr_reader   :log

    # @note
    #   Ruby (standard) logger levels:
    #     off:   absolutely nothing
    #     fatal: an unhandleable error that results in a program crash
    #     error: a handleable error condition
    #     warn:  a warning
    #     info:  generic (useful) information about system operation
    #     debug: low-level information for developers
    #
    #   DataObjects::Logger::LEVELS[:off, :fatal, :error, :warn, :info, :debug]
    LEVELS =
    {
      :off   => 99999,
      :fatal => 7,
      :error => 6,
      :warn  => 4,
      :info  => 3,
      :debug => 0
    }

    def level=(new_level)
      @level = LEVELS[new_level.to_sym]
      reset_methods(:close)
    end

    private

    # The idea here is that instead of performing an 'if' conditional check on
    # each logging we do it once when the log object is setup
    def set_write_method
      @log.instance_eval do

        # Determine if asynchronous IO can be used
        def aio?
          @aio = !RUBY_PLATFORM.match(/java|mswin/) &&
          !(@log == STDOUT) &&
          @log.respond_to?(:write_nonblock)
        end

        # Define the write method based on if aio an be used
        undef write_method if defined? write_method
        if aio?
          alias :write_method :write_nonblock
        else
          alias :write_method :write
        end
      end
    end

    def initialize_log(log)
      close if @log # be sure that we don't leave open files laying around.
      @log = log || "log/dm.log"
    end

    def reset_methods(o_or_c)
      if o_or_c == :open
        alias internal_push push_opened
      elsif o_or_c == :close
        alias internal_push push_closed
      end
    end

    def push_opened(string)
      message = Time.now.httpdate
      message << delimiter
      message << string
      message << "\n" unless message[-1] == ?\n
      @buffer << message
      flush # Force a flush for now until we figure out where we want to use the buffering.
    end

    def push_closed(string)
      unless @log.respond_to?(:write)
        log = Pathname(@log)
        log.dirname.mkpath
        @log = log.open('a')
        @log.sync = true
      end
      set_write_method
      reset_methods(:open)
      push(string)
    end

    alias internal_push push_closed

    def prep_msg(message, level)
      level << delimiter << message
    end

    public

    # To initialize the logger you create a new object, proxies to set_log.
    #   DataObjects::Logger.new(log{String, IO},level{Symbol, String})
    #
    # @param log<IO,String>        either an IO object or a name of a logfile.
    # @param log_level<String>     the message string to be logged
    # @param delimiter<String>     delimiter to use between message sections
    # @param log_creation<Boolean> log that the file is being created
    def initialize(*args)
      set_log(*args)
    end

    # To replace an existing logger with a new one:
    #  DataObjects::Logger.set_log(log{String, IO},level{Symbol, String})
    #
    #
    # @param log<IO,String>        either an IO object or a name of a logfile.
    # @param log_level<Symbol>     a symbol representing the log level from
    #   {:off, :fatal, :error, :warn, :info, :debug}
    # @param delimiter<String>     delimiter to use between message sections
    # @param log_creation<Boolean> log that the file is being created
    def set_log(log, log_level = :off, delimiter = " ~ ", log_creation = false)
      delimiter    ||= " ~ "

      if log_level && LEVELS[log_level.to_sym]
        self.level = log_level.to_sym
      else
        self.level = :debug
      end

      @buffer    = []
      @delimiter = delimiter

      initialize_log(log)

      DataObjects.logger = self

      self.info("Logfile created") if log_creation
    end

    # Flush the entire buffer to the log object.
    #   DataObjects.logger.flush
    #
    def flush
      return unless @buffer.size > 0
      @log.write_method(@buffer.slice!(0..-1).to_s)
    end

    # Close and remove the current log object.
    #   DataObjects.logger.close
    #
    def close
      flush
      @log.close if @log.respond_to?(:close)
      @log = nil
    end

    # Appends a string and log level to logger's buffer.

    # @note
    #   Note that the string is discarded if the string's log level less than the
    #   logger's log level.
    # @note
    #   Note that if the logger is aio capable then the logger will use
    #   non-blocking asynchronous writes.
    #
    # @param level<Fixnum>  the logging level as an integer
    # @param string<String> the message string to be logged
    def push(string)
      internal_push(string)
    end
    alias << push

    # Generate the following logging methods for DataObjects.logger as described
    # in the API:
    #  :fatal, :error, :warn, :info, :debug
    #  :off only gets an off? method
    LEVELS.each_pair do |name, number|
      unless name.to_sym == :off
        class_eval <<-EOS, __FILE__, __LINE__
          # DOC
          def #{name}(message)
            self.<<( prep_msg(message, "#{name}") ) if #{name}?
          end
        EOS
      end

      class_eval <<-EOS, __FILE__, __LINE__
        # DOC
        def #{name}?
          #{number} >= level
        end
      EOS
    end

  end # class Logger
end # module DataObjects
