module Rack::Mount
  module Analysis
    class Histogram < Hash #:nodoc:
      attr_reader :count

      def initialize
        @count = 0
        super(0)
      end

      def <<(value)
        @count += 1
        self[value] += 1 if value
      end

      def select_upper
        values = sort_by { |_, value| value }
        values.reverse!
        values = values.select { |_, value| value >= count / size }
        values.map! { |key, _| key }
        values
      end
    end
  end
end
