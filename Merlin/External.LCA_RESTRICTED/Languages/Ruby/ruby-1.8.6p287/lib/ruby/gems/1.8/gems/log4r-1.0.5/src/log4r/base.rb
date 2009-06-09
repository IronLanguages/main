# :nodoc:
require "log4r/config"

module Log4r
  ALL = 0
  LNAMES = ['ALL']

  # Defines the log levels of the Log4r module at runtime. It is given
  # either the default level spec (when root logger is created) or the
  # user-specified level spec (when Logger.custom_levels is called).
  #
  # The last constant defined by this method is OFF. Other level-sensitive 
  # parts of the code check to see if OFF is defined before deciding what 
  # to do. The typical action would be to force the creation of RootLogger 
  # so that the custom levels get loaded and business can proceed as usual.
  #
  # For purposes of formatting, a constant named MaxLevelLength is defined
  # in this method. It stores the max level name string size.

  def Log4r.define_levels(*levels) #:nodoc:
    return if const_defined? :OFF
    for i in 0...levels.size
      name = levels[i].to_s
      module_eval "#{name} = #{i} + 1; LNAMES.push '#{name}'"
    end
    module_eval %{
      LNAMES.push 'OFF'
      LEVELS = LNAMES.size
      OFF = LEVELS - 1
      MaxLevelLength = Log4rTools.max_level_str_size
    }
  end

  # Some common functions 
  class Log4rTools
    # Raises ArgumentError if level argument is an invalid level. Depth
    # specifies how many trace entries to remove.
    def self.validate_level(level, depth=0)
      unless valid_level?(level)
        raise ArgumentError, "Log level must be in 0..#{LEVELS}",
              caller[1..-(depth + 1)]
      end
    end
    
    def self.valid_level?(lev)
      not lev.nil? and lev.kind_of?(Numeric) and lev >= ALL and lev <= OFF
    end
    
    def self.max_level_str_size #:nodoc:
      size = 0
      LNAMES.each {|i| size = i.length if i.length > size}
      size
    end
  
    # Shortcut for decoding 'true', 'false', true, false or nil into a bool
    # from a hash parameter. E.g., it looks for true/false values for
    # the keys 'symbol' and :symbol.

    def self.decode_bool(hash, symbol, default)
      data = hash[symbol]
      data = hash[symbol.to_s] if data.nil?
      return case data
        when 'true',true then true
        when 'false',false then false
        else default
        end
    end

    # Splits comma-delimited lists with arbitrary \s padding
    def self.comma_split(string)
      string.split(/\s*,\s*/).collect {|s| s.strip}
    end
  end
end
