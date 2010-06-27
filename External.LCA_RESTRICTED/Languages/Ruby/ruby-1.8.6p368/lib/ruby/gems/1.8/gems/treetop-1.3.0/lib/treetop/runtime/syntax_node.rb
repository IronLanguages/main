module Treetop
  module Runtime
    class SyntaxNode
      attr_reader :input, :interval, :elements
      attr_accessor :parent

      def initialize(input, interval, elements = nil)
        @input = input
        @interval = interval
        if @elements = elements
          elements.each do |element|
            element.parent = self
          end
        end
      end

      def terminal?
        @elements.nil?
      end

      def nonterminal?
        !terminal?
      end

      def text_value
        input[interval]
      end

      def empty?
        interval.first == interval.last && interval.exclude_end?
      end

      def extension_modules
        local_extensions =
          class <<self
            included_modules-Object.included_modules
          end
        if local_extensions.size > 0
          local_extensions
        else
          []    # There weren't any; must be a literal node
        end
      end

      def inspect(indent="")
        em = extension_modules
        interesting_methods = methods-[em.last ? em.last.methods : nil]-self.class.instance_methods
        im = interesting_methods.size > 0 ? " (#{interesting_methods.join(",")})" : ""
        tv = text_value
        tv = "...#{tv[-20..-1]}" if tv.size > 20

        indent +
        self.class.to_s.sub(/.*:/,'') +
          em.map{|m| "+"+m.to_s.sub(/.*:/,'')}*"" +
          " offset=#{interval.first}" +
          ", #{tv.inspect}" +
          im +
          (elements && elements.size > 0 ?
            ":" +
              (@elements||[]).map{|e|
          begin
            "\n"+e.inspect(indent+"  ")
          rescue  # Defend against inspect not taking a parameter
            "\n"+indent+" "+e.inspect
          end
              }.join("") :
            ""
          )
      end
    end
  end
end
