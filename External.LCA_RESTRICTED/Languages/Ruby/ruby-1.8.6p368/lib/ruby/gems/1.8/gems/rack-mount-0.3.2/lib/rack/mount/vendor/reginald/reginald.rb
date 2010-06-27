module Reginald
  autoload :Alternation, 'reginald/alternation'
  autoload :Anchor, 'reginald/anchor'
  autoload :Atom, 'reginald/atom'
  autoload :Character, 'reginald/character'
  autoload :CharacterClass, 'reginald/character_class'
  autoload :Collection, 'reginald/collection'
  autoload :Expression, 'reginald/expression'
  autoload :Group, 'reginald/group'
  autoload :Parser, 'reginald/parser'

  class << self
    begin
      eval('foo = /(?<foo>.*)/').named_captures

      def regexp_supports_named_captures?
        true
      end
    rescue SyntaxError, NoMethodError
      def regexp_supports_named_captures?
        false
      end
    end

    def parse(regexp)
      Parser.parse_regexp(regexp)
    end

    def compile(source)
      regexp = Regexp.compile(source)
      expression = parse(regexp)
      Regexp.compile(expression.to_s(true), expression.options)
    end
  end
end
