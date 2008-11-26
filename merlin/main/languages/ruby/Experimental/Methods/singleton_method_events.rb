module Kernel
  def singleton_method_added name
    puts "#{self}##{name}"
  end
end

class C
  class << self
    class << self
      def bar
      end
    end
    
    def foo
    end
  end
end

require 'yaml'
YAML.methods()
