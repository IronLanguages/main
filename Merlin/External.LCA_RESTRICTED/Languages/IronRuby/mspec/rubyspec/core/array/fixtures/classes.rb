module ArraySpecs
  not_compliant_on :rubinius do
    def self.max_32bit_size
      2**32/4
    end

    def self.max_64bit_size
      2**64/8
    end
  end

  deviates_on :rubinius do
    def self.max_32bit_size
      2**30-1
    end

    def self.max_64bit_size
      2**62-1
    end
  end

  def self.frozen_array
    @frozen_array ||= [1,2,3]
    @frozen_array.freeze
    @frozen_array
  end

  def self.recursive_array
    a = [1, 'two', 3.0]
    tmp = [ a ]
    a << tmp
    a
  end

  def self.empty_recursive_array
    a = []
    a << a
    a
  end

  def self.recursive_arrays
    a = [1, 'two', 3.0]
    tmp1 = [ a ]
    b = [ tmp1 ]
    tmp2 = [ b ]
    a << tmp2
    return a, b
  end

  class MyArray < Array; end

  class Sexp < Array
    def initialize(*args)
      super(args)
    end
  end

  # TODO: replace specs that use this with #should_not_receive(:to_ary)
  # expectations on regular objects (e.g. Array instances).
  class ToAryArray < Array
    def to_ary() ["to_ary", "was", "called!"] end
  end

  class MyRange < Range; end

  class AssocKey
    def ==(other); other == 'it'; end
  end

  class D
    def <=>(obj)
      return 4 <=> obj unless obj.class == D
      0
    end
  end

  class SubArray < Array
    def initialize(*args)
      ScratchPad.record args
    end
  end
end
