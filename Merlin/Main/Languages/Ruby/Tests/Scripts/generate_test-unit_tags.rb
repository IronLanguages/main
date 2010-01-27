# Requiring this file causes Test::Unit failures to be printed in a format which
# disables the failing tests by monkey-patching the failing test method to a nop
#
# Note that this will only detect deterministic test failures. Sporadic
# non-deterministic test failures will have to be tracked separately

require 'test/unit/ui/console/testrunner'

def test_method_name(fault)
  match = 
    / (test.*) # method name
      \(
      (\w+) # testcase class name
      \)
    /x.match(fault.test_name)
  if match and match.size == 3
    [match[1], match[2]]
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

class Test::Unit::UI::Console::TestRunner
  def finished(elapsed_time)
    nl
    
    faults_by_testcase_class = {}
    @faults.each_with_index do |fault, index|
      testcase_class = test_method_name(fault)[1]
      faults_by_testcase_class[testcase_class] = [] if not faults_by_testcase_class.has_key? testcase_class
      faults_by_testcase_class[testcase_class] << fault
    end
    
    faults_by_testcase_class.each_key do |testcase_class|
      testcase_faults = faults_by_testcase_class[testcase_class]
      ensure_single_fault_per_method_name testcase_faults
      puts "    disable #{testcase_class}, "
      testcase_faults.each do |fault|
        method_name = test_method_name(fault)[0]
        commented_message = fault.message[0..400]
        if fault.respond_to? :exception
          commented_message += "\n" + fault.exception.backtrace[0..2].join("\n")
        end
        commented_message = commented_message.gsub(/^(.*)$/, '      # \1')
        puts commented_message
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
        puts "      #{method_name}#{comma_separator}"
      end
      nl
    end
  end
end

if $0 == __FILE__
  # Dummy example for testing
  require 'test/unit'  
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
