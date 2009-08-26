# :nodoc:
module Log4r

  class Outputter
    # Retrieve an outputter.
    def self.[](name)
    out = @@outputters[name]
      if out.nil?
        return case name
        when 'stdout' then StdoutOutputter.new 'stdout'
        when 'stderr' then StderrOutputter.new 'stderr'
        else nil end
      end          
      out
    end
    def self.stdout; Outputter['stdout'] end
    def self.stderr; Outputter['stderr'] end
    # Set an outputter.
    def self.[]=(name, outputter)
      @@outputters[name] = outputter
    end
    # Yields each outputter's name and reference.
    def self.each
      @@outputters.each {|name, outputter| yield name, outputter}
    end
    def self.each_outputter
      @@outputters.each_value {|outputter| yield outputter}
    end
  end
end
