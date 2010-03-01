module Reginald
  class CharacterClass < Character
    ALNUM = new(':alnum:').freeze
    ALPHA = new(':alpha:').freeze
    ASCII = new(':ascii:').freeze
    BLANK = new(':blank:').freeze
    CNTRL = new(':cntrl:').freeze
    DIGIT = new(':digit:').freeze
    GRAPH = new(':graph:').freeze
    LOWER = new(':lower:').freeze
    PRINT = new(':print:').freeze
    PUNCT = new(':punct:').freeze
    SPACE = new(':space:').freeze
    UPPER = new(':upper:').freeze
    WORD = new(':word:').freeze
    XDIGIT = new(':xdigit:').freeze

    def ignorecase=(ignorecase)
      if to_s !~ /\A\[:.*:\]\Z/
        super
      end
    end

    attr_accessor :negate

    def negated?
      negate ? true : false
    end

    def literal?
      false
    end

    def bracketed?
      value != '.' && value !~ /^\\[dDsSwW]$/
    end

    def to_s(parent = false)
      if bracketed?
        if !parent && ignorecase
          "(?i-mx:[#{negate && '^'}#{value}])#{quantifier}"
        else
          "[#{negate && '^'}#{value}]#{quantifier}"
        end
      else
        super
      end
    end

    def include?(char)
      re = quantifier ? to_s.sub(/#{Regexp.escape(quantifier)}$/, '') : to_s
      Regexp.compile("\\A#{re}\\Z").match(char)
    end

    def eql?(other)
      super && negate == other.negate
    end

    def freeze
      negate.freeze
      super
    end
  end
end
