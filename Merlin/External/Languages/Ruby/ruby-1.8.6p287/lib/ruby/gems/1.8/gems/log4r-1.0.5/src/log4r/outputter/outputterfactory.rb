# :nodoc:
# Version: $Id: outputterfactory.rb,v 1.3 2002/01/28 16:05:05 cepheus Exp $

require "log4r/base"
require "log4r/repository"
require "log4r/logger"

module Log4r
class Outputter

  class OutputterFactory #:nodoc:
    include Singleton
    
      # handles two cases: logging above a level (no second arg specified)
      # or logging a set of levels (passed into the second argument)
      def self.create_methods(out, levels=nil)
        Logger.root # force levels to be loaded

        # first, undefine all the log levels
        for mname in LNAMES
          undefine_log(mname.downcase, out)
        end
        if not levels.nil? and levels.include? OFF
          raise TypeError, "Can't log only_at OFF", caller[1..-1]
        end
        return out if out.level == OFF

        if levels.nil? # then define the log methods for lev >= outlev
          for lev in out.level...LEVELS
            define_log(LNAMES[lev].downcase, lev, out)
          end
        else # define the logs only for assigned levels
          for lev in levels
            define_log(LNAMES[lev].downcase, lev, out)
          end
        end
        return out
      end

      # we need to synch the actual write/format for thread safteyness
      def self.define_log(mname, level, out)
        return if mname == 'off' || mname == 'all'
        mstr = %-
          def out.#{mname}(logevent)
            canonical_log(logevent)
          end
        -
        module_eval mstr
      end
      
      def self.undefine_log(mname, out)
        return if mname == 'off' || mname == 'all'
        mstr = "def out.#{mname}(logevent); end"
        module_eval mstr
      end
  end

end
end
