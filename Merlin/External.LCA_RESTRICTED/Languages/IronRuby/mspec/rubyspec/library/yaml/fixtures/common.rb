require 'yaml'
require File.dirname(__FILE__) + '/strings'

$test_file = tmp("yaml_test_file")
$test_parse_file = File.dirname(__FILE__) + "/test_yaml.yml"

module YamlSpecs
  def self.get_a_node
    io = StringIO.new($test_yaml_string)
    YAML.each_node(io) { |node| return node }
  end
  
  def self.write_to_emitter(out, h)
    out.map(nil, nil) do |map|
      h.each_pair { |k, v| map.add(k, v) }
    end  
  end
  
  class StringSubclass < String
  end
  
  class RangeSubclass < Range
  end
  
  class RegexpSubclass < Regexp
  end
  
  class ArraySubclass < Array
  end
  
  class HashSubclass < Hash
  end
  
  class TimeSubclass < Time
  end
  
  class DateSubclass < Date
  end
  
  class Outer
    def initialize
      @outer1 = 1
      @outer2 = Inner.new
    end
  end

  class Inner
    def initialize
      @inner1 = 1
      @inner2 = 2
    end
  end

  class OuterToYaml
    def initialize
      @outer1 = 1
      @outer2 = InnerToYaml.new
    end
    
    def inner
      @outer2
    end
  end

  class InnerToYaml
    def initialize
      @inner1 = 1
      @inner2 = 2
    end

    def to_yaml(emitter)
      if ScratchPad.recorded
        ScratchPad.recorded[:emitter] = emitter
        if emitter.respond_to? :level 
          ScratchPad.recorded[:level] = emitter.level
        end
      end
      
      node = YAML::quick_emit(nil, emitter) do |out|
        out.map("tag:ruby.yaml.org,2002:object:YamlSpecs::InnerToYaml", to_yaml_style) do |map|
          map.add("inner1", @inner1)
          map.add("inner2", @inner2)
        end
      end
      
      if ScratchPad.recorded
        ScratchPad.recorded[:result] = node
      end
      
      node
    end
  end
end
