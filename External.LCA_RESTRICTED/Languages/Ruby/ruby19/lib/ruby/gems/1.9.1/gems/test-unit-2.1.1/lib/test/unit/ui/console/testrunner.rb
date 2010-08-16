#--
#
# Author:: Nathaniel Talbott.
# Copyright::
#   * Copyright (c) 2000-2003 Nathaniel Talbott. All rights reserved.
#   * Copyright (c) 2008-2009 Kouhei Sutou <kou@clear-code.com>
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
            @indent = 0
            @top_level = true
            @faults = []
          end

          private
          def setup_mediator
            super
            output_setup_end
          end

          def output_setup_end
            suite_name = @suite.to_s
            suite_name = @suite.name if @suite.kind_of?(Module)
            output("Loaded suite #{suite_name}")
          end

          def attach_to_mediator
            @mediator.add_listener(TestResult::FAULT, &method(:add_fault))
            @mediator.add_listener(TestRunnerMediator::STARTED, &method(:started))
            @mediator.add_listener(TestRunnerMediator::FINISHED, &method(:finished))
            @mediator.add_listener(TestCase::STARTED, &method(:test_started))
            @mediator.add_listener(TestCase::FINISHED, &method(:test_finished))
            @mediator.add_listener(TestSuite::STARTED, &method(:test_suite_started))
            @mediator.add_listener(TestSuite::FINISHED, &method(:test_suite_finished))
          end
          
          def add_fault(fault)
            @faults << fault
            output_progress(fault.single_character_display, fault_color(fault))
            @already_outputted = true if fault.critical?
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
            @faults.each_with_index do |fault, index|
              nl
              output_single("%3d) " % (index + 1))
              output_fault(fault)
            end
            nl
            output("Finished in #{elapsed_time} seconds.")
            nl
            output(@result, result_color)
            output("%g%% passed" % @result.pass_percentage, result_color)
          end

          def output_fault(fault)
            if @use_color and fault.is_a?(Failure) and
                fault.inspected_expected and fault.inspected_actual
              output_single(fault.label, fault_color(fault))
              output(":")
              output_fault_backtrace(fault)
              output_fault_message(fault)
            else
              label, detail = format_fault(fault).split(/\r?\n/, 2)
              output(label, fault_color(fault))
              output(detail)
            end
          end

          def output_fault_backtrace(fault)
            backtrace = fault.location
            if backtrace.size == 1
              output(fault.test_name +
                     backtrace[0].sub(/\A(.+:\d+).*/, ' [\\1]') +
                     ":")
            else
              output(fault.test_name)
              backtrace.each_with_index do |entry, i|
                if i.zero?
                  prefix = "["
                  postfix = ""
                elsif i == backtrace.size - 1
                  prefix = " "
                  postfix = "]:"
                else
                  prefix = " "
                  postfix = ""
                end
                output("    #{prefix}#{entry}#{postfix}")
              end
            end
          end

          def output_fault_message(fault)
            output(fault.user_message) if fault.user_message
            output_single("<")
            output_single(fault.inspected_expected, color("pass"))
            output("> expected but was")
            output_single("<")
            output_single(fault.inspected_actual, color("failure"))
            output(">")
            from, to = prepare_for_diff(fault.expected, fault.actual)
            if from and to
              differ = ColorizedReadableDiffer.new(from.split(/\r?\n/),
                                                   to.split(/\r?\n/),
                                                   self)
              if differ.need_diff?
                output("")
                output("diff:")
                differ.diff
              end
            end
          end

          def format_fault(fault)
            fault.long_display
          end

          def test_started(name)
            return unless output?(VERBOSE)

            name = name.sub(/\(.+?\)\z/, '')
            right_space = 8 * 2
            left_space = @progress_row_max - right_space
            left_space = left_space - indent.size - name.size
            tab_stop = "\t" * ([left_space - 1, 0].max / 8)
            output_single("#{indent}#{name}:#{tab_stop}", nil, VERBOSE)
            @test_start = Time.now
          end

          def test_finished(name)
            unless @already_outputted
              output_progress(".", color("pass"))
            end
            @already_outputted = false

            return unless output?(VERBOSE)

            output(": (%f)" % (Time.now - @test_start), nil, VERBOSE)
          end

          def test_suite_started(name)
            if @top_level
              @top_level = false
              return
            end

            output_single(indent, nil, VERBOSE)
            if /\A[A-Z]/ =~ name
              _color = color("case")
            else
              _color = color("suite")
            end
            output_single(name, _color, VERBOSE)
            output(": ", nil, VERBOSE)
            @indent += 2
          end

          def test_suite_finished(name)
            @indent -= 2
          end

          def indent
            if output?(VERBOSE)
              " " * @indent
            else
              ""
            end
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
            _color = @color_scheme[name]
            _color ||= @color_scheme["success"] if name == "pass"
            _color ||= ColorScheme.default[name]
            _color
          end

          def fault_color(fault)
            color(fault.class.name.split(/::/).last.downcase)
          end

          def result_color
            color(@result.status)
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

        class ColorizedReadableDiffer < Diff::ReadableDiffer
          def initialize(from, to, runner)
            @runner = runner
            super(from, to)
          end

          def need_diff?(options={})
            operations.each do |tag,|
              return true if [:replace, :equal].include?(tag)
            end
            false
          end

          private
          def output_single(something, color=nil)
            @runner.send(:output_single, something, color)
          end

          def output(something, color=nil)
            @runner.send(:output, something, color)
          end

          def color(name)
            @runner.send(:color, name)
          end

          def cut_off_ratio
            0
          end

          def default_ratio
            0
          end

          def tag(mark, color_name, contents)
            _color = color(color_name)
            contents.each do |content|
              output_single(mark, _color)
              output_single(" ")
              output(content)
            end
          end

          def tag_deleted(contents)
            tag("-", "diff-deleted-tag", contents)
          end

          def tag_inserted(contents)
            tag("+", "diff-inserted-tag", contents)
          end

          def tag_equal(contents)
            tag(" ", "normal", contents)
          end

          def tag_difference(contents)
            tag("?", "diff-difference-tag", contents)
          end

          def diff_line(from_line, to_line)
            to_operations = []
            from_line, to_line, _operations = line_operations(from_line, to_line)

            no_replace = true
            _operations.each do |tag,|
              if tag == :replace
                no_replace = false
                break
              end
            end

            output_single("?", color("diff-difference-tag"))
            output_single(" ")
            _operations.each do |tag, from_start, from_end, to_start, to_end|
              from_width = compute_width(from_line, from_start, from_end)
              to_width = compute_width(to_line, to_start, to_end)
              case tag
              when :replace
                output_single(from_line[from_start...from_end],
                              color("diff-deleted"))
                if (from_width < to_width)
                  output_single(" " * (to_width - from_width))
                end
                to_operations << Proc.new do
                  output_single(to_line[to_start...to_end],
                                color("diff-inserted"))
                  if (to_width < from_width)
                    output_single(" " * (from_width - to_width))
                  end
                end
              when :delete
                output_single(from_line[from_start...from_end],
                              color("diff-deleted"))
                unless no_replace
                  to_operations << Proc.new {output_single(" " * from_width)}
                end
              when :insert
                if no_replace
                  output_single(to_line[to_start...to_end],
                                color("diff-inserted"))
                else
                  output_single(" " * to_width)
                  to_operations << Proc.new do
                    output_single(to_line[to_start...to_end],
                                  color("diff-inserted"))
                  end
                end
              when :equal
                output_single(from_line[from_start...from_end])
                unless no_replace
                  to_operations << Proc.new {output_single(" " * to_width)}
                end
              else
                raise "unknown tag: #{tag}"
              end
            end
            output("")

            unless to_operations.empty?
              output_single("?", color("diff-difference-tag"))
              output_single(" ")
              to_operations.each do |operation|
                operation.call
              end
              output("")
            end
          end
        end
      end
    end
  end
end

if __FILE__ == $0
  Test::Unit::UI::Console::TestRunner.start_command_line_test
end
