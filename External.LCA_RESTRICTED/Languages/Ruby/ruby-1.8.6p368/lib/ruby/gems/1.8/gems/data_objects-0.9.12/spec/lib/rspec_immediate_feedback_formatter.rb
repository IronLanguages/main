require 'spec/runner/formatter/base_text_formatter'

# Code is based on standard SpecdocFormatter, but will print full error details as soon as they are found.
# Successful or pending examples are written only as a dot in the output. Header is only printed if errors occur.
#
# To use it, add the following to your spec/spec.opts:
#  --require
#  lib/rspec_immediate_feedback_formatter.rb
#  --format
#  Spec::Runner::Formatter::ImmediateFeedbackFormatter

module Spec
  module Runner
    module Formatter
      class ImmediateFeedbackFormatter < BaseTextFormatter

        def add_example_group(example_group)
          super
          @current_group = example_group.description
        end

        def example_failed(example, counter, failure)
          if @current_group
            output.puts
            output.puts @current_group
            @current_group = nil  # only print the group name once
          end

          message = if failure.expectation_not_met?
            "- #{example.description} (FAILED - #{counter})"
          else
            "- #{example.description} (ERROR - #{counter})"
          end

          output.puts(red(message))
          dump_failure(counter, failure)  # dump stacktrace immediately
          output.flush
        end

        def example_passed(*)
          output.print green('.')
          output.flush
        end

        def example_pending(*)
          super
          output.print yellow('*')
          output.flush
        end
      end
    end
  end
end
