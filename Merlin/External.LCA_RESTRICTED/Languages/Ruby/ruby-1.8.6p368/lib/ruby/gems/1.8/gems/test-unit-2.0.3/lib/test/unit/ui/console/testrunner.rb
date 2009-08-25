#--
#
# Author:: Nathaniel Talbott.
# Copyright:: Copyright (c) 2000-2003 Nathaniel Talbott. All rights reserved.
# License:: Ruby license.

require 'test/unit/color-scheme'
require 'test/unit/ui/testrunner'
require 'test/unit/ui/testrunnermediator'
require 'test/unit/ui/console/outputlevel'

module Test
  module Unit
    module UI
      module Console

        # Runs a Test::Unit::TestSuite on the console.
        class TestRunner < UI::TestRunner
          include OutputLevel

          # Creates a new TestRunner for running the passed
          # suite. If quiet_mode is true, the output while
          # running is limited to progress dots, errors and
          # failures, and the final result. io specifies
          # where runner output should go to; defaults to
          # STDOUT.
          def initialize(suite, options={})
            super
            @output_level = @options[:output_level] || NORMAL
            @output = @options[:output] || STDOUT
            @use_color = @options[:use_color]
            @use_color = guess_color_availability if @use_color.nil?
            @color_scheme = @options[:color_scheme] || ColorScheme.default
            @reset_color = Color.new("reset")
            @progress_row = 0
            @progress_row_max = @options[:progress_row_max]
            @progress_row_max ||= guess_progress_row_max
            @already_outputted = false
            @faults = []
          end

          # Begins the test run.
          def start
            setup_mediator
            attach_to_mediator
            return start_mediator
          end

          private
          def setup_mediator
            @mediator = create_mediator(@suite)
            output_setup_end
          end

          def output_setup_end
            suite_name = @suite.to_s
            suite_name = @suite.name if @suite.kind_of?(Module)
            output("Loaded suite #{suite_name}")
          end

          def create_mediator(suite)
            return TestRunnerMediator.new(suite)
          end
          
          def attach_to_mediator
            @mediator.add_listener(TestResult::FAULT, &method(:add_fault))
            @mediator.add_listener(TestRunnerMediator::STARTED, &method(:started))
            @mediator.add_listener(TestRunnerMediator::FINISHED, &method(:finished))
            @mediator.add_listener(TestCase::STARTED, &method(:test_started))
            @mediator.add_listener(TestCase::FINISHED, &method(:test_finished))
          end
          
          def start_mediator
            return @mediator.run_suite
          end
          
          def add_fault(fault)
            @faults << fault
            output_progress(fault.single_character_display, fault_color(fault))
            @already_outputted = true
          end
          
          def started(result)
            @result = result
            output_started
          end

          def output_started
            output("Started")
          end

          def finished(elapsed_time)
            nl if output?(NORMAL) and !output?(VERBOSE)
            nl
            output("Finished in #{elapsed_time} seconds.")
            @faults.each_with_index do |fault, index|
              nl
              output_single("%3d) " % (index + 1))
              label, detail = format_fault(fault).split(/\r?\n/, 2)
              output(label, fault_color(fault))
              output(detail)
            end
            nl
            output(@result, result_color)
          end

          def format_fault(fault)
            fault.long_display
          end
          
          def test_started(name)
            output_single(name + ": ", nil, VERBOSE)
          end
          
          def test_finished(name)
            unless @already_outputted
              output_progress(".", color("success"))
            end
            nl(VERBOSE)
            @already_outputted = false
          end
          
          def nl(level=NORMAL)
            output("", nil, level)
          end
          
          def output(something, color=nil, level=NORMAL)
            return unless output?(level)
            output_single(something, color, level)
            @output.puts
          end
          
          def output_single(something, color=nil, level=NORMAL)
            return false unless output?(level)
            if @use_color and color
              something = "%s%s%s" % [color.escape_sequence,
                                      something,
                                      @reset_color.escape_sequence]
            end
            @output.write(something)
            @output.flush
            true
          end

          def output_progress(mark, color=nil)
            if output_single(mark, color, PROGRESS_ONLY)
              return unless @progress_row_max > 0
              @progress_row += mark.size
              if @progress_row >= @progress_row_max
                nl unless @output_level == VERBOSE
                @progress_row = 0
              end
            end
          end

          def output?(level)
            level <= @output_level
          end

          def color(name)
            @color_scheme[name] || ColorScheme.default[name]
          end

          def fault_color(fault)
            color(fault.class.name.split(/::/).last.downcase)
          end

          def result_color
            if @result.passed?
              if @result.pending_count > 0
                color("pending")
              elsif @result.omission_count > 0
                color("omission")
              elsif @result.notification_count > 0
                color("notification")
              else
                color("success")
              end
            elsif @result.error_count > 0
              color("error")
            elsif @result.failure_count > 0
              color("failure")
            end
          end

          def guess_color_availability
            return false unless @output.tty?
            case ENV["TERM"]
            when /term(?:-color)?\z/, "screen"
              true
            else
              return true if ENV["EMACS"] == "t"
              false
            end
          end

          def guess_progress_row_max
            term_width = guess_term_width
            if term_width.zero?
              if ENV["EMACS"] == "t"
                -1
              else
                79
              end
            else
              term_width
            end
          end

          def guess_term_width
            Integer(ENV["TERM_WIDTH"] || 0)
          rescue ArgumentError
            0
          end
        end
      end
    end
  end
end

if __FILE__ == $0
  Test::Unit::UI::Console::TestRunner.start_command_line_test
end
