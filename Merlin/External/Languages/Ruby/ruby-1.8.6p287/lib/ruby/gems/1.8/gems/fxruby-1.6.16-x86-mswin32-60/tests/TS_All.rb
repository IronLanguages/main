require 'fox16'
require 'test/unit'

class TS_All
  def TS_All.suite
    suite = Test::Unit::TestSuite.new
    Object.constants.sort.each do |k|
      next if /^TC_/ !~ k
      constant = Object.const_get(k)
      if constant.kind_of?(Class)
#       puts "adding tests for #{constant.to_s}"
	suite << constant.suite
      end
    end
    suite
  end
end

if __FILE__ == $0
  require 'test/unit/ui/console/testrunner'
  Dir.glob("TC_*.rb").each do |testcase|
    require "#{testcase}"
  end
  Test::Unit::UI::Console::TestRunner.run(TS_All)
end
