require 'stringio'
require 'test/unit/ui/tap/testrunner'

class TestTap < Test::Unit::TestCase
  def test_run
    fail_line = nil
    test_case = Class.new(Test::Unit::TestCase) do
      def test_success
        assert_equal(3, 1 + 2)
      end

      def test_fail; assert_equal(3, 1 - 2); end; fail_line = __LINE__
    end
    output = StringIO.new
    runner = Test::Unit::UI::Tap::TestRunner.new(test_case.suite,
                                                 :output => output)
    result = runner.start; start_line = __LINE__
    assert_equal(<<-EOR, output.string.gsub(/[\d\.]+ seconds/, "0.001 seconds"))
1..2
not ok 1 - test_fail(): <3> expected but was
# Failure:
# test_fail()
#     [#{__FILE__}:#{fail_line}:in `test_fail'
#      #{__FILE__}:#{start_line}:in `test_run']:
# <3> expected but was
# <-1>.
ok 2 - test_success()
# Finished in 0.001 seconds.
# 2 tests, 2 assertions, 1 failures, 0 errors, 0 pendings, 0 omissions, 0 notifications
EOR
    assert_true(result.passed?)
  end
end
