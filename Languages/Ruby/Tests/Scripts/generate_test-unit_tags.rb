# Requiring this file causes Test::Unit failures to be printed in a format which
# disables the failing tests by monkey-patching the failing test method to a nop
#
# Note that this will only detect deterministic test failures. Sporadic
# non-deterministic test failures will have to be tracked separately

require 'test/unit/ui/console/testrunner'
require 'stringio'

def test_method_name(fault)
  match = 
    / (test.*) # method name
      \(
      (.+) # testcase class name (in parenthesis)
      \)
    /x.match(fault.test_name)

  if match and match.size == 3
    method_name, class_name = match[1], match[2]
    if !(class_name =~ /^[\w:]+$/) and defined? ActiveSupport::Testing::Declarative
      # class_name might be a descriptive string specified with ActiveSupport::Testing::Declarative.describe
      ObjectSpace.each_object(Class) do |klass|
        if klass.respond_to? :name
          if klass.name == class_name
            class_name = klass.to_s
            break
          end
        end
      end
    end
    [method_name, class_name]
  else
    warn "Could not parse test name : #{fault.test_name}"
    [fault.test_name, "Could not parse test name"] 
  end
end

# Some tests have both a failure and an error
def ensure_single_fault_per_method_name(faults)
  method_names = []
  faults.reject! do |f|
    method_name = test_method_name(f)[0]
    if method_names.include? method_name
      true
    else
      method_names << method_name
      false
    end
  end
end

class TagGenerator

  class << self
    attr :test_file, true # Name of the file with UnitTestSetup and disabled tags
    attr :initial_tag_generation, true
  end

  def self.write_tags(new_tags)
    existing_content = nil
    File.open(TagGenerator.test_file) do |file|
      existing_content = file.read
    end
    
    method_name = TagGenerator.initial_tag_generation ? "disable_tests" : "disable_unstable_tests"
    pos = /(def #{method_name}(\(\))?)\n/ =~ existing_content
    exit "Please add a placeholder for #{method_name}()" if not pos
    pos = pos + $1.size

    File.open(TagGenerator.test_file, "w+") do |file|
      file.write existing_content[0..pos]
      file.write new_tags.string
      file.write existing_content[pos..-1]
      puts "Added #{new_tags.size} tags to #{TagGenerator.test_file}"
    end
  rescue
    puts
    puts "Could not write tags..."
    puts new_tags.string
  end
    
  def self.finished(faults, elapsed_time)
    return if faults.size == 0
    if TagGenerator.test_file
      new_tags = StringIO.new("", "a+")
      TagGenerator.output_tags faults, new_tags
      TagGenerator.write_tags new_tags
    else
      TagGenerator.output_tags faults, $stdout
    end    
  end
  
  def self.output_tags(faults, output)
    output.puts()
    
    faults_by_testcase_class = {}
    faults.each_with_index do |fault, index|
      testcase_class = test_method_name(fault)[1]
      faults_by_testcase_class[testcase_class] = [] if not faults_by_testcase_class.has_key? testcase_class
      faults_by_testcase_class[testcase_class] << fault
    end
    
    faults_by_testcase_class.each_key do |testcase_class|
      testcase_faults = faults_by_testcase_class[testcase_class]
      ensure_single_fault_per_method_name testcase_faults
      output.puts "    disable #{testcase_class}, "
      testcase_faults.each do |fault|
        method_name = test_method_name(fault)[0]
        commented_message = fault.message[0..400]
        if fault.respond_to? :exception
          backtrace = fault.exception.backtrace[0..2].join("\n")
          # Sometimes backtrace is an Array of size 1, where the first and only 
          # element is the string for the full backtrace
          backtrace = backtrace[0..1000]
          commented_message += "\n" + backtrace
        end
        commented_message = commented_message.gsub(/^(.*)$/, '      # \1')
        output.puts commented_message
        if fault == testcase_faults.last
          comma_separator = ""
        else
          comma_separator = ","
        end
        if method_name =~ /^[[:alnum:]_]+[?!]?$/
          method_name = ":" + method_name.to_s
        else
          method_name = "#{method_name.dump}"
        end
        output.puts "      #{method_name}#{comma_separator}"
      end
      output.puts()
    end
  end
end

class Test::Unit::UI::Console::TestRunner
  def finished(elapsed_time)
    TagGenerator.finished(@faults, elapsed_time)
    nl
    output(@result, result_color)
  end
end

if $0 == __FILE__
  # Dummy examples for testing
  
  if RUBY_VERSION == "1.8.7" or RUBY_VERSION =~ /1.9/
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'activesupport', "= 3.0.pre"
    require 'active_support'
    require 'active_support/test_case'
    class TestCaseWithDescription < ActiveSupport::TestCase
      describe "Hello there @%$"
      def test_1() assert(false) end
    end
  end

  class ExampleTest < Test::Unit::TestCase
    def teardown() 
      if @teardown_error
        @teardown_error = false
        raise "error during teardown"
      end
    end
    def raise_exception() raise "hi\nthere\n\nyou" end
    def test_1!() assert(false) end   
    def test_2?() raise_exception end
    def test_3() assert(true) end
    def test_4() @teardown_error = true; assert(false) end
    define_method("test_\"'?:-@2") { assert(false) }
  end  
end
