require 'test/unit/ui/console/testrunner'

module Test::Unit::UI           # :nodoc:
  module SpecDox                # :nodoc:
    class TestRunner < Test::Unit::UI::Console::TestRunner
      protected
      def setup_mediator
        @mediator = create_mediator(@suite)
      end

      def add_fault(fault)
        if fault.kind_of? Test::Spec::Disabled
          @disabled += 1
          output_no_nl " (disabled)"
        elsif fault.kind_of? Test::Spec::Empty
          @empty += 1
          output_no_nl " (empty)"
        else
          @faults << fault
          word = fault.class.name[/(.*::)?(.*)/, 2].upcase
          output_no_nl " (#{word} - #{@faults.size})"
        end
      end
      
      def started(result)
        @io ||= @output
        @result = result
        @context = nil
        @contexts = []
        @disabled = 0
        @empty = 0
        indent 0
      end
      
      def finished(elapsed_time)
        nl
        output "Finished in #{elapsed_time} seconds."
        @faults.each_with_index do |fault, index|
          nl
          output("%3d) %s" % [index + 1, fault.long_display])
        end
        nl
        output_result
      end

      def output_result
        if @disabled > 0
          disabled = ", #{@disabled} disabled"
        else
          disabled = ""
        end

        if @empty > 0
          empty = ", #{@empty} empty"
        else
          empty = ""
        end

        r = ("%d specifications#{disabled}#{empty} " +
             "(%d requirements), %d failures") % [
               @result.run_count, @result.assertion_count, @result.failure_count]
        r << ", #{@result.error_count} errors"  if @result.error_count > 0
        output r
      end
      
      def test_started(name)
        return  if special_test? name

        contextname, @specname = unmangle name
        return  if contextname.nil? || @specname.nil?

        if @context != contextname
          @context = contextname

          @old_contexts = @contexts
          @contexts = @context.split("\t")

          common = 0
          @contexts.zip(@old_contexts) { |a, b|
            break  if a != b
            common += 1
          }

          nl  if common == 0

          @contexts[common..-1].each_with_index { |head, i|
            indent common + i
            output_heading head
          }
        end
        
        @assertions = @result.assertion_count
        @prevdisabled = @disabled
        output_item @specname
      end
      
      def test_finished(name)
        return  if special_test? name

        # Did any assertion run?
        if @assertions == @result.assertion_count && @prevdisabled == @disabled
          add_fault Test::Spec::Empty.new(@specname)
        end

        # Don't let empty contexts clutter up the output.
        nl  unless name =~ /\Adefault_test\(/
      end

      def output_no_nl(something, level=NORMAL)
        @io.write(something) if (output?(level))
        @io.flush
      end

      def output_item(item)
        output_no_nl "#{@prefix}- #{item}"
      end

      def output_heading(heading)
        output "#{@prefix}#{heading}"
      end

      def unmangle(name)
        if name =~ /\Atest_spec \{(.*?)\} \d+ \[(.*)\]/
          contextname = $1
          specname = $2
        elsif name =~ /test_(.*?)\((.*)\)$/
          specname = $1
          contextname = $2

          contextname.gsub!(/^Test\B|\BTest$/, '')
          specname.gsub!(/_/, ' ')
        else
          contextname = specname = nil
        end

        [contextname, specname]
      end

      def indent(depth)
        @indent = depth
        @prefix = "  " * depth
      end

      def special_test?(name)
        name =~ /\Atest_spec \{.*?\} (-1 BEFORE|AFTER) ALL\(/
      end
    end
  end
end
