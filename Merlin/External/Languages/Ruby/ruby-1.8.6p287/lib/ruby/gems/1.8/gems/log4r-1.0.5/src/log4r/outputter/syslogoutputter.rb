# :include: ../rdoc/syslogoutputter
#
# Version:: $Id: syslogoutputter.rb,v 1.5 2004/03/17 20:18:00 fando Exp $
# Author:: Steve Lumos
# Author:: Leon Torres

require 'log4r/formatter/formatter'
require 'log4r/outputter/outputter'
require 'log4r/configurator'
require 'syslog'

module Log4r

  # syslog needs to set the custom levels
  if LNAMES.size > 1
    raise LoadError, "Must let syslogger.rb define custom levels"
  end
  # tune log4r to syslog priorities
  Configurator.custom_levels("DEBUG", "INFO", "NOTICE", "WARNING", "ERR", "CRIT", "ALERT", "EMERG")

  class SyslogOutputter < Outputter
    include Syslog::Constants

    # maps log4r levels to syslog priorities (logevents never see ALL and OFF)
    SYSLOG_LEVELS = [
        nil, 
        LOG_DEBUG, 
        LOG_INFO, 
        LOG_WARNING, 
        LOG_ERR, 
        LOG_CRIT, 
        nil
    ]

    # There are 3 hash arguments
    #
    # [<tt>:ident</tt>]     syslog ident, defaults to _name
    # [<tt>:logopt</tt>]    syslog logopt, defaults to LOG_PID | LOG_CONS
    # [<tt>:facility</tt>]  syslog facility, defaults to LOG_USER
    def initialize(_name, hash={})
      super(_name, hash)
      ident = (hash[:ident] or hash['ident'] or _name)
      logopt = (hash[:logopt] or hash['logopt'] or LOG_PID | LOG_CONS).to_i
      facility = (hash[:facility] or hash['facility'] or LOG_USER).to_i
      @syslog = Syslog.open(ident, logopt, facility)
    end

    def closed?
      return !@syslog.opened?
    end
    
    def close
      @syslog.close unless @syslog.nil?
      @level = OFF
      OutputterFactory.create_methods(self)
      Logger.log_internal {"Outputter '#{@name}' closed Syslog and set to OFF"}
    end

    private

    def canonical_log(logevent)
      pri = SYSLOG_LEVELS[logevent.level]
      o = logevent.data
      if o.kind_of? Exception then
        msg = "#{o.class} at (#{o.backtrace[0]}): #{o.message}"
      elsif o.respond_to? :to_str then
        msg = o.to_str
      else
        msg = o.inspect
      end
      
      @syslog.log(pri, '%s', msg)
    end
  end
end
