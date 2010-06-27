begin
  include Microsoft::Scripting::Silverlight
  SILVERLIGHT = true
rescue LoadError
  SILVERLIGHT = false
end

$: << 'rblib'

if SILVERLIGHT
  include System::Windows
  include System::Windows::Controls

  dyneng = DynamicApplication.current.engine
  engine = dyneng.runtime.get_engine("ruby")
  $repl = Repl.show(engine, dyneng.create_scope)

  $stdout = $repl.output_buffer
  $stderr = $repl.output_buffer

  Application.current.root_visual = UserControl.new  
end

require 'bacon'

begin
  require 'python'
rescue LoadError
  # ignore
end

class BaconRunner
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

    def run(cfg)
      config cfg
      current.run_tests
    end

    def current
      @instance ||= BaconRunner.new
    end
  end

  # 
  # Test Running
  #
  # TODO need a way to walk all *_test.rb files in tests directory
  def run_tests
    BaconRunner.get_config.each do |tests|
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
    BaconRunner.execute_at_exit_blocks
  end
end

#
# Redefine at_exit to simply collect the blocks passed to it
#
BaconRunner.at_exit_blocks = []

module Kernel
  def at_exit(&block)
    BaconRunner.at_exit_blocks.push block
  end
end

Bacon.summary_on_exit
