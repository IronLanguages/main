module A
  module B
    module C
      p Module.nesting
    end  
  end
  
  p Module.nesting
end

p Module.nesting

module A
  module B::C::D
    p Module.nesting
  end  
end