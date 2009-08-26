require 'test/spec/dox'

module Test::Unit::UI           # :nodoc:
  module RDox                   # :nodoc:
    class TestRunner < Test::Unit::UI::SpecDox::TestRunner
      def output_heading(heading)
        output "#{@headprefix} #{heading}"
      end
      
      def output_item(item)
        output_no_nl "* #{item}"
      end
      
      def finished(elapsed_time)
        nl
        output_result
      end

      def indent(depth)
        @prefix = ""
        @headprefix = "==" + "=" * depth
      end
    end
  end
end
