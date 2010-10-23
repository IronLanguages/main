require 'rubygems'
require 'active_support'
require 'active_support/core_ext/logger'

module TestLogger
  class CustomFormatter < Logger::Formatter
    def call(severity, time, progname, msg)
      "[#{
        time.strftime("%Y-%m-%d %H:%M:%S")
      }] #{severity} #{
        "((#{progname})" if progname
      } #{msg2str(msg)}\n"
    end
  end

  class GlobalLogger
    def log
      @log ||= begin
        _log = Logger.new(STDOUT)
        _log.level = Logger::INFO
        _log.formatter = CustomFormatter.new
        _log
      end
    end
  
    def filelog
      @filelog ||= begin
        _filelog = Logger.new(File.join(Dir.pwd, 'test.log'))
        _filelog.level = Logger::INFO
        _filelog.formatter = CustomFormatter.new
        _filelog
      end
    end
    
    def self.current
      @current ||= GlobalLogger.new
    end
  end

  def log
    GlobalLogger.current.log
  end
  
  def filelog
    GlobalLogger.current.filelog
  end

  def set_log_level(level)
    unless Logger::Severity.const_defined? level
      raise "\"#{level}\" not a valid log level; pick from: #{Logger::Severity.constants.join(', ')}."
    end
    level = Logger::Severity.const_get(level)
    log.level = level
    filelog.level = level
  end

  def info(msg)
    log.info msg
    filelog.info msg
    nil
  end

  def fatal(msg)
    log.fatal msg
    filelog.fatal msg
    nil
  end
  
  def error(msg)
    log.error msg
    filelog.error msg
    nil
  end
  
  def warn(msg)
    log.warn msg
    filelog.warn msg
    nil
  end
  
  def debug(msg)
    log.debug msg
    filelog.debug msg
    nil
  end
end