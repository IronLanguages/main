require 'test/unit/ui/testrunnerutilities'

module Test
  module Unit
    module UI
      class TestRunner
        extend TestRunnerUtilities

        def initialize(suite, options={})
          if suite.respond_to?(:suite)
            @suite = suite.suite
          else
            @suite = suite
          end
          @options = options
        end

        def diff_target_string?(string)
          Assertions::AssertionMessage.diff_target_string?(string)
        end

        def prepare_for_diff(from, to)
          Assertions::AssertionMessage.prepare_for_diff(from, to)
        end
      end
    end
  end
end
