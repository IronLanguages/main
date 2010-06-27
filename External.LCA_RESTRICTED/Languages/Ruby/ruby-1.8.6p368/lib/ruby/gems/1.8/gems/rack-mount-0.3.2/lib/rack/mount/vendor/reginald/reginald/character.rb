module Reginald
  class Character < Atom
    attr_accessor :quantifier

    def literal?
      quantifier.nil? && !ignorecase
    end

    def to_s(parent = false)
      if !parent && ignorecase
        "(?i-mx:#{value})#{quantifier}"
      else
        "#{value}#{quantifier}"
      end
    end

    def to_regexp
      Regexp.compile("\\A#{to_s(true)}\\Z", ignorecase)
    end

    def match(char)
      to_regexp.match(char)
    end

    def include?(char)
      if ignorecase
        value.downcase == char.downcase
      else
        value == char
      end
    end

    def eql?(other)
      super && quantifier.eql?(other.quantifier)
    end

    def freeze
      quantifier.freeze
      super
    end
  end
end
