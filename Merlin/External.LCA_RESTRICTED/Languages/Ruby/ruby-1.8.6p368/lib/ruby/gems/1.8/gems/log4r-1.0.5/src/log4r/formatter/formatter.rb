# :include: ../rdoc/formatter
#
# Version:: $Id: formatter.rb,v 1.7 2003/09/01 22:33:20 cepheus Exp $

require "singleton"

require "log4r/base"

module Log4r

  # Formatter is an abstract class and a null object
  class Formatter
    def initialize(hash={})
    end
    # Define this method in a subclass to format data.
    def format(logevent)
    end
  end

  # SimpleFormatter produces output like this:
  # 
  #   WARN loggername> Danger, Will Robinson, danger!
  #
  # Does not write traces and does not inspect objects.
  
  class SimpleFormatter < Formatter
    def format(event)
      sprintf("%*s %s> %s\n", MaxLevelLength, LNAMES[event.level], 
              event.name, event.data)
    end
  end

  # BasicFormatter produces output like this:
  # 
  #   WARN loggername: I dropped my Wookie!
  #   
  # Or like this if trace is on:
  # 
  #   WARN loggername(file.rb at 12): Hot potato!
  #   
  # Also, it will pretty-print any Exception it gets and
  # +inspect+ everything else.
  #
  # Hash arguments include:
  #
  # +depth+::  How many lines of the stacktrace to display.

  class BasicFormatter < SimpleFormatter
    @@basicformat = "%*s %s"

    def initialize(hash={})
      @depth = (hash[:depth] or hash['depth'] or 7).to_i
    end
    
    def format(event)
      buff = sprintf(@@basicformat, MaxLevelLength, LNAMES[event.level],
             event.name)
      buff += (event.tracer.nil? ? "" : "(#{event.tracer[0]})") + ": "
      buff += format_object(event.data) + "\n"
      buff
    end

    # Formats data according to its class:
    #
    # String::     Prints it out as normal.
    # Exception::  Produces output similar to command-line exceptions.
    # Object::     Prints the type of object, then the output of
    #              +inspect+. An example -- Array: [1, 2, 3]

    def format_object(obj)
      if obj.kind_of? Exception
        return "Caught #{obj.class}: #{obj.message}\n\t" +\
               obj.backtrace[0...@depth].join("\n\t")
      elsif obj.kind_of? String
        return obj
      else # inspect the object
        return "#{obj.class}: #{obj.inspect}"
      end
    end
  end

  # Formats objects the same way irb does:
  #
  #   loggername:foo.rb in 12> 
  #   [1, 3, 4]
  #   loggername:foo.rb in 13> 
  #   {1=>"1"}
  #
  # Strings don't get inspected. just printed. The trace is optional.

  class ObjectFormatter < Formatter
    def format(event)
      buff = event.logger.name
      buff += (event.tracer.nil? ? "" : ":#{event.tracer[0]}") + ">\n"
      buff += (event.data.kind_of?(String) ? event.data : event.data.inspect)
      buff += "\n"
    end
  end
  
  # Outputters that don't define a Formatter will get this, which
  # is currently BasicFormatter
  class DefaultFormatter < BasicFormatter
  end
  
end
