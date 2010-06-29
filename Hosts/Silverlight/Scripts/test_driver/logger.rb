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

  def log
    @log ||= begin
      _log = Logger.new(STDOUT)
      _log.level = Logger::DEBUG
      _log.formatter = CustomFormatter.new
      _log
    end
  end
end