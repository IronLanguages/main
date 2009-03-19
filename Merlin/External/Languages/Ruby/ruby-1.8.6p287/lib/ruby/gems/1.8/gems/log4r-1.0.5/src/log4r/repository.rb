# :nodoc:
# Version:: $Id: repository.rb,v 1.10 2002/08/20 07:40:26 cepheus Exp $

require "singleton"

module Log4r
class Logger

  # The repository stores a Hash of loggers keyed to their fullnames and
  # provides a few functions to reduce the code bloat in log4r/logger.rb.
  # This class is supposed to be transparent to end users, hence it is
  # a class within Logger. If anyone knows how to make this private,
  # let me know.
  
  class Repository # :nodoc:
    include Singleton
    attr_reader :loggers
    
    def initialize
      @loggers = Hash.new
    end

    def self.[](fullname)
      instance.loggers[fullname]
    end

    def self.[]=(fullname, logger)
      instance.loggers[fullname] = logger
    end
  
    # Retrieves all children of a parent
    def self.all_children(parent)
      # children have the parent name + delimiter in their fullname
      daddy = parent.name + Private::Config::LoggerPathDelimiter
      for fullname, logger in instance.loggers
        yield logger if parent.is_root? || fullname =~ /#{daddy}/
      end
    end

    # when new loggers are introduced, they may get inserted into
    # an existing inheritance tree. this method
    # updates the children of a logger to link their new parent
    def self.reassign_any_children(parent)
      for fullname, logger in instance.loggers
        next if logger.is_root?
        logger.parent = parent if logger.path =~ /^#{parent.fullname}$/
      end
    end
    
    # looks for the first defined logger in a child's path 
    # or nil if none found (which will then be rootlogger)
    def self.find_ancestor(path)
      arr = path.split Log4rConfig::LoggerPathDelimiter
      logger = nil
      while arr.size > 0 do
        logger = Repository[arr.join(Log4rConfig::LoggerPathDelimiter)]
        break unless logger.nil?
        arr.pop
      end
      logger
    end

  end
end
end
