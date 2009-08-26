# :nodoc:
require "log4r/outputter/iooutputter"

module Log4r
  # Same as IOOutputter(name, $stdout)
  class StdoutOutputter < IOOutputter
    def initialize(_name, hash={})
      super(_name, $stdout, hash)
    end
  end

  # Same as IOOutputter(name, $stderr)
  class StderrOutputter < IOOutputter
    def initialize(_name, hash={})
      super(_name, $stderr, hash)
    end
  end
end
