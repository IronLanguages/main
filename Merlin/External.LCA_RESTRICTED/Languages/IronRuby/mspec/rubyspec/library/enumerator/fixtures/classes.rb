module EnumSpecs
  class Numerous
    include Enumerable
    
    attr :list
    
    def initialize(*list)
      ScratchPad.record []
      @list = list.empty? ? [2, 5, 3, 6, 1, 4] : list
    end
    
    def each
      @list.each do |i|
        ScratchPad << i
        yield i
      end
    end 
  end
end
