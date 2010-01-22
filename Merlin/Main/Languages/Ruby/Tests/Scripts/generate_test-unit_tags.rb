# Requiring this file causes Test::Unit failures to be printed in a format which
# disables the failing tests by monkey-patching the failing test method to a nop
#
# Note that this will only detect deterministic test failures. Sporadic
# non-deterministic test failures will have to be tracked separately

require 'test/unit/ui/console/testrunner'

def test_method_name(fault)
  match = 
    / (test_[^()]+) # method name
      \(
      (\w+) # testcase class name
      \)
    /x.match(fault.test_name)
  raise "could not parse : #{fault.test_name}" if not match or match.size != 3
  [match[1], match[2]]
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
      puts "    disable #{testcase_class}, "
      testcase_faults.each do |fault|
        method_name = test_method_name(fault)[0]
        commented_message = fault.message[0..400]
        commented_message = commented_message.gsub(/^(.*)$/, '      # \1')
        puts commented_message
        if fault == testcase_faults.last
          comma_separator = ""
        else
          comma_separator = ","
        end
        puts "      :#{method_name}#{comma_separator}"
      end
      nl
    end
  end
end

if $0 == __FILE__
  # Dummy example for testing
  require 'test/unit'  
  class ExampleTest < Test::Unit::TestCase
    def test_1!() assert(false) end   
    def test_2?() raise "hi\nthere\nyou" end
    def test_3() assert(true) end
  end  
end
