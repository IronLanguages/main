# TODO should this depend on Microsoft::Scripting::Silverlight at all?

include Microsoft::Scripting::Silverlight
SILVERLIGHT = true

#
# 'bacon' is the spec framework used for the tests
#
require 'bacon'

# 
# Helper for running python code from Ruby
#
begin
  require 'python'
rescue LoadError
  # ignore
end

class BaconSL
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
      engine = DynamicApplication.current.engine.runtime.get_engine("ruby")
      Repl.show(engine, engine.create_scope)

      $stdout = Repl.current.output_buffer
      $stderr = Repl.current.output_buffer

      BaconSL.current.run_tests
    end

    def current
      @instance ||= BaconSL.new
    end
  end

  # 
  # Test Running
  #
  # TODO need a way to walk all *_test.rb files in tests directory
  def run_tests
    BaconSL.get_config.each do |tests|
      if tests.kind_of?(String)
        begin
          load "#{tests}.rb"
          loaded = true
        rescue LoadError
        end
        if !loaded
          puts "Warning: #{tests} was not found -- skipping"
        end
      else
        test_type = tests.first
        tests.last.each do |file|
          loaded = false
          ["#{test_type}/test_#{file}.rb", "#{test_type}/#{file}_test.rb"].each do |pth|
            prepend = File.dirname(DynamicApplication.current ? 
                DynamicApplication.current.entry_point.to_s :
                '')
            pth = "#{prepend}/#{pth}" if prepend != '.'
            begin
              load pth
              loaded = true
              next
            rescue LoadError
            end 
          end
          if !loaded
            puts "Warning: #{test_type} -- #{file} was not found -- skipping"
          end
        end
      end
    end
    BaconSL.execute_at_exit_blocks
  end
end

#
# Redefine at_exit to simply collect the blocks passed to it
#
BaconSL.at_exit_blocks = []

module Kernel
  def at_exit(&block)
    BaconSL.at_exit_blocks.push block
  end
end

Bacon.summary_on_exit
