require 'rake'
require 'rake/tasklib'
require 'fileutils'

namespace 'SilverlightTestTask' do
  task :load_driver do
    require '../Scripts/test_driver'
  end

  task :ironruby_check do
    raise "Must run Silverlight tests with IronRuby" if !defined?(RUBY_ENGINE) || RUBY_ENGINE != 'ironruby'
  end
end

module Silverlight
  class TestTask < Rake::TaskLib
    attr_accessor :browsers, :tests

    PRE = ['SilverlightTestTask:ironruby_check', 'SilverlightTestTask:load_driver']

    def initialize(name)
      @name, @test_args = self.class.process_name(name)
      unless @description = Rake.application.last_description
        @description = "Run tests for #{@name}"
      end
      @last_sub_description = nil
      @browsers = []
      @log_level = ENV['LOG_LEVEL'] || 'INFO'
      @tests = {}
      @dir = Dir.pwd
      yield self if block_given?
      define
    end

    def self.process_name(name)
      task_args = {}
      if name.kind_of?(String)
        task_args = {name => PRE}
      elsif name.kind_of?(Hash)
        task_args = name
        name = name.keys.first
        PRE.each do |pre|
          task_args[task_args.keys.first] << pre
        end
      else
        task_args = name
      end
      [name, task_args]
    end

    def define
      __define__(@description, @test_args) do
        run @tests
      end
    end

    class SubTest 
      attr_accessor :html_filename
      attr_accessor :number_of_expected_results

      def initialize
        @number_of_expected_results = 1
      end
    end

    def sub_desc(description)
      @last_sub_description = description
    end

    def sub_test(name)
      name, test_args = self.class.process_name(name)
      st = SubTest.new
      yield st if block_given?
      tests[st.html_filename] = st.number_of_expected_results
      namespace @name do
        __define__(@last_sub_description, test_args) do
          run st.html_filename => st.number_of_expected_results
        end
      end
      @last_sub_description = nil
    end
    
    def run(tests)
      FileUtils.cd @dir do
        TestDriver.run :browsers => @browsers, :tests => tests, :log_level => @log_level
      end
    end
    
    private
      
      def __define__(description, task_args, &block)
        desc description if description
        task task_args, &block
      end
  end
end
