module M
  class << self
    alias :old_append_features :append_features
    
    def included mod
      puts "M included to #{mod}"
    end
   
    def append_features mod
      puts "M append-features to #{mod}: #{mod.included_modules.inspect}"
    end
  end
  
  def goo
  end
end

module N
  class << self
    alias :old_append_features :append_features
  
    def included mod
      puts "N included to #{mod}"
    end 
    
    def append_features mod
      puts "N append-features to #{mod}: #{mod.included_modules.inspect}"
      old_append_features mod
      puts "new: #{mod.included_modules.inspect}"
    end
  end
  
  def foo
  end
end

def getM
  puts 'M'
  M
end

def getN
  puts 'N'
  N
end

module Bar
end

class C
  include getM, getN
  include Enumerable
  include Bar
  
  p included_modules
  p ancestors
end

