begin
  include Microsoft::Scripting::Silverlight
  SILVERLIGHT = true
rescue LoadError
  SILVERLIGHT = false
end

$: << 'rblib'

if SILVERLIGHT
  $: << 'lib'
  require 'test_results'
  
  include System
  include System::Windows
  include System::Windows::Browser
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
    BaconResults.broadcast
  end
end

class BaconResults
  class << self
    def broadcast
      r = get_results
      TestResults.broadcast(r, pass?(r), get_output)
    end
    
    def get_results
      results_str = get_results_string
      %W(tests assertions skips specifications requirements failures errors).inject({}) do |results, ttype|
        results_str.scan(/(\d*)\s#{ttype}/) do |m|
          results[ttype.to_sym] = m.first.split(/[ (]/).last.to_i
        end
        results
      end
    end
  
    def pass?(results)
      results.keys.size > 0 &&
      results.has_key?(:failures) && results.has_key?(:errors) &&
      results[:failures] && results[:errors] &&
      results[:failures] == 0 && results[:errors] == 0
    end
  
    def get_output
      if output_element = $repl.output
        output_element.html
      end
    end

    def get_results_string
      results_element = $repl.output.children.select do |i|
        i.tag_name == 'span' && i.css_class == ''
      end[-2]
      results_element ?
        HttpUtility.html_decode(results_element.html).to_s :
        nil
    end
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
