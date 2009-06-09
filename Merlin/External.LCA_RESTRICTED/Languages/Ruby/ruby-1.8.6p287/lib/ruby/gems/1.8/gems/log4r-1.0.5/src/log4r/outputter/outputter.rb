# :include: ../rdoc/outputter
#
# == Other Info
#
# Version:: $Id: outputter.rb,v 1.6 2003/09/12 23:55:43 fando Exp $
# Author:: Leon Torres <leon@ugcs.caltech.edu>

require "thread"

require "log4r/outputter/outputterfactory"
require "log4r/formatter/formatter"
require "log4r/staticlogger"

module Log4r

  class Outputter
    attr_reader :name, :level, :formatter
    @@outputters = Hash.new

    # An Outputter needs a name. RootLogger will be loaded if not already
    # done. The hash arguments are as follows:
    # 
    # [<tt>:level</tt>]       Logger level. Optional, defaults to root level
    # [<tt>:formatter</tt>]   A Formatter. Defaults to DefaultFormatter

    def initialize(_name, hash={})
      if _name.nil?
        raise ArgumentError, "Bad arguments. Name and IO expected.", caller
      end
      @name = _name
      validate_hash(hash)
      @@outputters[@name] = self
    end

    # dynamically change the level
    def level=(_level)
      Log4rTools.validate_level(_level)
      @level = _level
      OutputterFactory.create_methods(self)
      Logger.log_internal {"Outputter '#{@name}' level is #{LNAMES[_level]}"}
    end

    # Set the levels to log. All others will be ignored
    def only_at(*levels)
      raise ArgumentError, "Gimme some levels!", caller if levels.empty?
      raise ArgumentError, "Can't log only_at ALL", caller if levels.include? ALL
      levels.each {|level| Log4rTools.validate_level(level)}
      @level = levels.sort.first
      OutputterFactory.create_methods self, levels
      Logger.log_internal {
        "Outputter '#{@name}' writes only on " +\
        levels.collect{|l| LNAMES[l]}.join(", ")
      }
    end

    # Dynamically change the formatter. You can just specify a Class
    # object and the formatter will invoke +new+ or +instance+
    # on it as appropriate.
 
    def formatter=(_formatter)
      if _formatter.kind_of?(Formatter)
        @formatter = _formatter
      elsif _formatter.kind_of?(Class) and _formatter <= Formatter
        if _formatter.respond_to? :instance
          @formatter = _formatter.instance
        else
          @formatter = _formatter.new
        end
      else
        raise TypeError, "Argument was not a Formatter!", caller
      end
      Logger.log_internal {"Outputter '#{@name}' using #{@formatter.class}"}
    end

    # Call flush to force an outputter to write out any buffered
    # log events. Similar to IO#flush, so use in a similar fashion.

    def flush
    end

    #########
    protected
    #########

    # Validates the common hash arguments. For now, that would be
    # +:level+, +:formatter+ and the string equivalents
    def validate_hash(hash)
      @mutex = Mutex.new
      # default to root level and DefaultFormatter
      if hash.empty?
        self.level = Logger.root.level
        @formatter = DefaultFormatter.new
        return
      end
      self.level = (hash[:level] or hash['level'] or Logger.root.level)
      self.formatter = (hash[:formatter] or hash['formatter'] or DefaultFormatter.new)
    end

    #######
    private
    #######

    # This method handles all log events passed to a typical Outputter. 
    # Overload this to change the overall behavior of an outputter. Make
    # sure that the new behavior is thread safe.

    def canonical_log(logevent)
      synch { write(format(logevent)) }
    end

    # Common method to format data. All it does is call the resident
    # formatter's format method. If a different formatting behavior is 
    # needed, then overload this method.

    def format(logevent)
      # @formatter is guaranteed to be DefaultFormatter if no Formatter
      # was specified
      @formatter.format(logevent)
    end

    # Abstract method to actually write the data to a destination.
    # Custom outputters should overload this to specify how the
    # formatted data should be written and to where. 
   
    def write(data)
    end

    def synch; @mutex.synchronize { yield } end
    
  end

end
