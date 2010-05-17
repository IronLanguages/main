class C
end

p x = C.new
class << x
  def to_s
    'XXX'
  end

  class << self    
    p self
  end
end

puts '----'

class << C
  p self
  class << self
    p self    
  end
end

puts '----'

module M
end

class << M
  p self
  class << self
    p self    
  end
end


puts '---'

class Module
  def to_s
    'goo'
  end
end

class << Class
  def to_s
    'hoo'
  end
end
        
p Class
p C
p M
class << M
  p self
  class << self
    p self
  end
end

