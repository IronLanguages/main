module Reginald
  class Expression < Collection
    attr_reader :ignorecase
    attr_accessor :multiline, :extended

    def self.reduce(expression_or_atom, atom = nil)
      if expression_or_atom.is_a?(Expression)
        expression_or_atom << atom if atom
        new(*expression_or_atom)
      elsif atom.nil?
        new(expression_or_atom)
      else
        new(expression_or_atom, atom)
      end
    end

    def initialize(*args)
      @multiline = @ignorecase = @extended = nil

      if args.length == 1 && args.first.instance_of?(Array)
        super(args.first)
      else
        args = args.map { |e| e.instance_of?(String) ? Character.new(e) : e }
        super(args)
      end
    end

    def ignorecase=(ignorecase)
      if @ignorecase.nil?
        super
        @ignorecase = ignorecase
      else
        false
      end
    end

    def literal?
      !ignorecase && all? { |e| e.literal? }
    end

    def options
      flag = 0
      flag |= Regexp::MULTILINE if multiline
      flag |= Regexp::IGNORECASE if ignorecase
      flag |= Regexp::EXTENDED if extended
      flag
    end

    def options=(flag)
      self.multiline  = flag & Regexp::MULTILINE != 0
      self.ignorecase = flag & Regexp::IGNORECASE != 0
      self.extended   = flag & Regexp::EXTENDED != 0
      nil
    end

    def to_s(parent = false)
      if parent || options == 0
        map { |e| e.to_s(parent) }.join
      else
        with, without = [], []
        multiline ? (with << 'm') : (without << 'm')
        ignorecase ? (with << 'i') : (without << 'i')
        extended ? (with << 'x') : (without << 'x')

        with = with.join
        without = without.any? ? "-#{without.join}" : ''

        "(?#{with}#{without}:#{map { |e| e.to_s(true) }.join})"
      end
    end

    def inspect
      "#<Expression #{to_s.inspect}>"
    end

    def casefold?
      ignorecase
    end

    def eql?(other)
      super &&
        !!self.multiline == !!other.multiline &&
        !!self.ignorecase == !!other.ignorecase &&
        !!self.extended == !!other.extended
    end
  end
end
