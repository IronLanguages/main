module EnumSpecs
  class Numerous

    include Enumerable
    attr_accessor :list
    def initialize(*list)
      @list = list.empty? ? [2, 5, 3, 6, 1, 4] : list
    end
    
    def each
      @list.each { |i| ScratchPad << i; yield i }
    end 
  end

end
