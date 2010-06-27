module Reginald
  class Atom < Struct.new(:value)
    attr_accessor :ignorecase

    def initialize(*args)
      @ignorecase = nil
      super
    end

    def literal?
      false
    end

    def casefold?
      ignorecase ? true : false
    end

    def to_s(parent = false)
      "#{value}"
    end

    def inspect
      "#<#{self.class.to_s.sub('Reginald::', '')} #{to_s.inspect}>"
    end

    def ==(other)
      case other
      when String
        other == to_s
      else
        eql?(other)
      end
    end

    def eql?(other)
      other.instance_of?(self.class) &&
        self.value.eql?(other.value) &&
        (!!self.ignorecase).eql?(!!other.ignorecase)
    end

    def freeze
      value.freeze
      super
    end
  end
end
