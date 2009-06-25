# TODO should this depend on Microsoft::Scripting::Silverlight at all?

include Microsoft::Scripting::Silverlight
SILVERLIGHT = true

#
# 'bacon' is the spec framework used for the tests
#
$:.unshift "#{File.dirname(__FILE__)}/eggs/lib/bacon/lib"
require 'bacon'

# 
# Helper for running python code from Ruby
#
$:.unshift "#{File.dirname(__FILE__)}/eggs"
begin
  require 'python'
rescue
  # ignore
end

#
# TODO better way to redirect output?
#
class IO
  def write(str)
    Repl.current.output_buffer.write(str)
  end
end

class Eggs
  class << self
    def at_exit_blocks
      @at_exit_blocks ||= []
      @at_exit_blocks
    end

    def at_exit_blocks=(value)
      @at_exit_blocks = value
    end

    def execute_at_exit_blocks
      while !at_exit_blocks.empty?
        at_exit_blocks.pop.call
      end
    end

    def config(options = {})
      @config = options
    end

    def get_config
      @config
    end

    def run(engine = nil)
      engine ? Repl.show(engine, engine.create_scope) : Repl.show
      Repl.current.input_buffer.write("Eggs.current.run_tests\n")
    end

    def current
      @instance ||= Eggs.new
    end
  end

  # 
  # Test Running
  #
  # TODO need a way to walk all *_test.rb files in tests directory
  def run_tests
    Eggs.get_config.each do |test_type, test_files|
      test_files.each do |file|
        loaded = false
        ["#{test_type}/#{file}_test.rb", "#{test_type}/test_#{file}.rb"].each do |pth|
          prepend = File.dirname(DynamicApplication.current ? 
              DynamicApplication.current.entry_point.to_s :
              '')
          pth = "#{prepend}/#{pth}" if prepend != '.'
          if !loaded && Package.get_file(pth)
            load pth 
            loaded = true
          end
        end
        raise "#{file} is not a known test (check your Eggs.config call)" unless loaded
      end
    end
    Eggs.execute_at_exit_blocks
  end
end

#
# Redefine at_exit to simply collect the blocks passed to it
#
Eggs.at_exit_blocks = []

module Kernel
  def at_exit(&block)
    Eggs.at_exit_blocks.push block
  end
end

Bacon.summary_on_exit

#
# 'mocha' is the mocking framework used for the tests
#
# Commented out since it's not being used
#$: << "lib/mocha/lib"
#require 'mocha_standalone'

