module Templater
  module Actions
    class Evaluation < Action
      attr_reader :generaor, :name, :options, :operation
      
      def initialize(generator, name, options = {}, &operation)
        @generator, @name     = generator, name
        @options              = options
        @operation            = operation
      end
      
      def render
        self.generator.instance_eval(&operation) || ''
      end

      def identical?
        false
      end
    end
  end
end
