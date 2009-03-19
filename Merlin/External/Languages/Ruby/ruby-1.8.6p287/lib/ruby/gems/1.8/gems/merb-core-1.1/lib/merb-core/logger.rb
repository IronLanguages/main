# Merb::Logger = Extlib::Logger

class Merb::Logger < Extlib::Logger
  # :api: public
  def verbose!(message, level = :warn)
    send("#{level}!", message) if Merb::Config[:verbose]
  end

  # :api: public
  def verbose(message, level = :warn)
    send(level, message) if Merb::Config[:verbose]
  end
end

# require "time" # httpdate
# ==== Public Merb Logger API
#
# To replace an existing logger with a new one:
#  Merb::Logger.set_log(log{String, IO},level{Symbol, String})
#
# Available logging levels are
#   Merb::Logger::{ Fatal, Error, Warn, Info, Debug }
#
# Logging via:
#   Merb.logger.fatal(message<String>,&block)
#   Merb.logger.error(message<String>,&block)
#   Merb.logger.warn(message<String>,&block)
#   Merb.logger.info(message<String>,&block)
#   Merb.logger.debug(message<String>,&block)
#
# Logging with autoflush:
#   Merb.logger.fatal!(message<String>,&block)
#   Merb.logger.error!(message<String>,&block)
#   Merb.logger.warn!(message<String>,&block)
#   Merb.logger.info!(message<String>,&block)
#   Merb.logger.debug!(message<String>,&block)
#
# Flush the buffer to
#   Merb.logger.flush
#
# Remove the current log object
#   Merb.logger.close
#
# ==== Private Merb Logger API
#
# To initialize the logger you create a new object, proxies to set_log.
#   Merb::Logger.new(log{String, IO},level{Symbol, String})
module Merb

  class Logger

    attr_accessor :level
    attr_accessor :delimiter
    attr_accessor :auto_flush
    attr_reader   :buffer
    attr_reader   :log
    attr_reader   :init_args

    # ==== Notes
    # Ruby (standard) logger levels:
    # :fatal:: An unhandleable error that results in a program crash
    # :error:: A handleable error condition
    # :warn:: A warning
    # :info:: generic (useful) information about system operation
    # :debug:: low-level information for developers
    Levels = Mash.new({
      :fatal => 7,
      :error => 6,
      :warn  => 4,
      :info  => 3,
      :debug => 0
    }) unless const_defined?(:Levels)

    @@mutex = {}

    public

    # To initialize the logger you create a new object, proxies to set_log.
    #
    # ==== Parameters
    # *args:: Arguments to create the log from. See set_logs for specifics.
    def initialize(*args)
      set_log(*args)
    end

    # Replaces an existing logger with a new one.
    #
    # ==== Parameters
    # log<IO, String>:: Either an IO object or a name of a logfile.
    # log_level<~to_sym>::
    #   The log level from, e.g. :fatal or :info. Defaults to :error in the
    #   production environment and :debug otherwise.
    # delimiter<String>::
    #   Delimiter to use between message sections. Defaults to " ~ ".
    # auto_flush<Boolean>::
    #   Whether the log should automatically flush after new messages are
    #   added. Defaults to false.
    def set_log(stream = Merb::Config[:log_stream],
      log_level = Merb::Config[:log_level],
      delimiter = Merb::Config[:log_delimiter],
      auto_flush = Merb::Config[:log_auto_flush])

      @buffer                   = []
      @delimiter                = delimiter
      @auto_flush               = auto_flush

      if Levels[log_level]
        @level                  = Levels[log_level]
      else
        @level                  = log_level
      end

      @log                      = stream
      @log.sync                 = true
      @mutex = (@@mutex[@log] ||= Mutex.new)
    end

    # Flush the entire buffer to the log object.
    def flush
      return unless @buffer.size > 0
      @mutex.synchronize do
        @log.write(@buffer.slice!(0..-1).to_s)
      end
    end

    # Close and remove the current log object.
    def close
      flush
      @log.close if @log.respond_to?(:close) && !@log.tty?
      @log = nil
    end

    # Appends a message to the log. The methods yield to an optional block and
    # the output of this block will be appended to the message.
    #
    # ==== Parameters
    # string<String>:: The message to be logged. Defaults to nil.
    #
    # ==== Returns
    # String:: The resulting message added to the log file.
    def <<(string = nil)
      message = ""
      message << delimiter
      message << string if string
      message << "\n" unless message[-1] == ?\n
      @buffer << message
      flush if @auto_flush

      message
    end
    alias :push :<<

    # Generate the logging methods for Merb.logger for each log level.
    Levels.each_pair do |name, number|
      class_eval <<-LEVELMETHODS, __FILE__, __LINE__

      # Appends a message to the log if the log level is at least as high as
      # the log level of the logger.
      #
      # ==== Parameters
      # string<String>:: The message to be logged. Defaults to nil.
      #
      # ==== Returns
      # self:: The logger object for chaining.
      def #{name}(message = nil)
        if #{number} >= level
          message = block_given? ? yield : message
          self << message if #{number} >= level
        end
        self
      end

      # Appends a message to the log if the log level is at least as high as
      # the log level of the logger. The bang! version of the method also auto
      # flushes the log buffer to disk.
      #
      # ==== Parameters
      # string<String>:: The message to be logged. Defaults to nil.
      #
      # ==== Returns
      # self:: The logger object for chaining.
      def #{name}!(message = nil)
        if #{number} >= level
          message = block_given? ? yield : message
          self << message if #{number} >= level
          flush if #{number} >= level
        end
        self
      end

      # ==== Returns
      # Boolean:: True if this level will be logged by this logger.
      def #{name}?
        #{number} >= level
      end
      LEVELMETHODS
    end

  end

end
