#--
#
# Author:: Nathaniel Talbott.
# Copyright:: Copyright (c) 2000-2002 Nathaniel Talbott. All rights reserved.
# License:: Ruby license.

module Test
  module Unit

    # Encapsulates a test failure. Created by Test::Unit::TestCase
    # when an assertion fails.
    class Failure
      attr_reader :test_name, :location, :message
      attr_reader :expected, :actual, :user_message
      attr_reader :inspected_expected, :inspected_actual

      SINGLE_CHARACTER = 'F'
      LABEL = "Failure"

      # Creates a new Failure with the given location and
      # message.
      def initialize(test_name, location, message, options={})
        @test_name = test_name
        @location = location
        @message = message
        @expected = options[:expected]
        @actual = options[:actual]
        @inspected_expected = options[:inspected_expected]
        @inspected_actual = options[:inspected_actual]
        @user_message = options[:user_message]
      end
      
      # Returns a single character representation of a failure.
      def single_character_display
        SINGLE_CHARACTER
      end

      def label
        LABEL
      end

      # Returns a brief version of the error description.
      def short_display
        "#@test_name: #{@message.split("\n")[0]}"
      end

      # Returns a verbose version of the error description.
      def long_display
        if location.size == 1
          location_display = location[0].sub(/\A(.+:\d+).*/, ' [\\1]')
        else
          location_display = "\n    [#{location.join("\n     ")}]"
        end
        "#{label}:\n#@test_name#{location_display}:\n#@message"
      end

      # Overridden to return long_display.
      def to_s
        long_display
      end

      def critical?
        true
      end

      def diff
        @diff ||= compute_diff
      end

      private
      def compute_diff
        Assertions::AssertionMessage.delayed_diff(@expected, @actual).inspect
      end
    end

    module FailureHandler
      class << self
        def included(base)
          base.exception_handler(:handle_assertion_failed_error)
        end
      end

      private
      def handle_assertion_failed_error(exception)
        return false unless exception.is_a?(AssertionFailedError)
        problem_occurred
        add_failure(exception.message, exception.backtrace,
                    :expected => exception.expected,
                    :actual => exception.actual,
                    :inspected_expected => exception.inspected_expected,
                    :inspected_actual => exception.inspected_actual,
                    :user_message => exception.user_message)
        true
      end

      def add_failure(message, backtrace, options={})
        failure = Failure.new(name, filter_backtrace(backtrace), message,
                              options)
        current_result.add_failure(failure)
      end
    end

    module TestResultFailureSupport
      attr_reader :failures

      # Records a Test::Unit::Failure.
      def add_failure(failure)
        @failures << failure
        notify_fault(failure)
        notify_changed
      end

      # Returns the number of failures this TestResult has
      # recorded.
      def failure_count
        @failures.size
      end

      def failure_occurred?
        not @failures.empty?
      end

      private
      def initialize_containers
        super
        @failures = []
        @summary_generators << :failure_summary
        @problem_checkers << :failure_occurred?
      end

      def failure_summary
        "#{failure_count} failures"
      end
    end
  end
end
