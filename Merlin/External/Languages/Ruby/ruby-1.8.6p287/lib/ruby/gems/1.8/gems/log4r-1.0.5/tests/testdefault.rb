# actually, tests only the following:
require "testlogger"
require "testoutputter"
require "testformatter"
require "testpatternformatter"

require "runit/testsuite"
require "runit/cui/testrunner"

class TestDefault
  def TestDefault.suite
    suite = TestSuite.new
    for k in Object.constants.sort
      next if /^Test/ !~ k
      const = Object.const_get(k)
      if const.kind_of?(Class) && const.superclass == RUNIT::TestCase
        suite.add(const.suite)
      end
    end
    suite
  end
end

CUI::TestRunner.run(TestDefault.suite)

