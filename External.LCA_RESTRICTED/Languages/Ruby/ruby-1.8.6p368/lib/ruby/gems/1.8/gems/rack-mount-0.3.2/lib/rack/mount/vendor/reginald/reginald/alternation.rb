module Reginald
  class Alternation < Collection
    def self.reduce(alternation_or_expression, expression)
      if alternation_or_expression.first.is_a?(Alternation)
        alternation_or_expression = alternation_or_expression.first
        alternation_or_expression << expression
        new(*alternation_or_expression)
      else
        new(alternation_or_expression, expression)
      end
    end

    def initialize(*args)
      if args.length == 1 && args.first.instance_of?(Array)
        super(args.first)
      else
        super(args)
      end
    end

    def literal?
      false
    end

    def options
      0
    end

    def to_s(parent = false)
      map { |e| e.to_s(parent) }.join('|')
    end

    def inspect
      to_s.inspect
    end
  end
end
