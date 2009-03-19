module Merb
  
  module ColorfulMessages
    # red
    def error(*messages)
      puts messages.map { |msg| "\033[1;31m#{msg}\033[0m" }
    end
    # yellow
    def warning(*messages)
      puts messages.map { |msg| "\033[1;33m#{msg}\033[0m" }
    end
    # green
    def success(*messages)
      puts messages.map { |msg| "\033[1;32m#{msg}\033[0m" }
    end

    alias_method :message, :success
  end

  module Generators
    
    extend Templater::Manifold
    
    desc <<-DESC
      Generate components for your application or entirely new applications.
    DESC
    
    class Generator < Templater::Generator
      
      include Merb::ColorfulMessages
      
      def initialize(*args)
        Merb::Config.setup({
          :log_level        => :fatal,
          :log_delimiter    => " ~ ",
          :log_auto_flush   => false,
          :reload_templates => false,
          :reload_classes   => false
        })

        Merb::BootLoader::Logger.run
        Merb::BootLoader::BuildFramework.run
        Merb::BootLoader::Dependencies.run
        
        super
        options[:orm] ||= Merb.orm
        options[:testing_framework] ||= Merb.test_framework
        options[:template_engine] ||= Merb.template_engine
      end
    
      # Inside a template, wraps a block of code properly in modules, keeping the indentation correct
      # 
      # @param modules<Array[#to_s]> an array of modules to use for nesting
      # @option indent<Integer> number of integers to indent the modules by
      def with_modules(modules, options={}, &block)
        indent = options[:indent] || 0
        text = capture(&block)
        modules.each_with_index do |mod, i|
          concat(("  " * (indent + i)) + "module #{mod}\n", block.binding)
        end
        text = Array(text).map{ |line| ("  " * modules.size) + line }.join
        concat(text, block.binding)
        modules.reverse.each_with_index do |mod, i|
          concat(("  " * (indent + modules.size - i - 1)) + "end # #{mod}\n", block.binding)
        end
      end
      
      # Returns a string of num times '..', useful for example in tests for namespaced generators
      # to find the spec_helper higher up in the directory structure.
      #
      # @param num<Integer> number of directories up
      # @return <String> concatenated string 
      def go_up(num)
        (["'..'"] * num).join(', ')
      end
    
      def self.source_root
        File.join(File.dirname(__FILE__), '..', 'generators', 'templates')
      end
    end
    
  end  
  
end
