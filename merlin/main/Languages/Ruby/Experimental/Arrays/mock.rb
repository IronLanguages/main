class I
  def initialize val
    @val = val
  end

  def to_int
    puts '!I to_int'
    @val
  end
  
  def respond_to? name
    puts "?I #{name}"
    super
  end
end

class S
  def initialize val
    @val = val
  end
  
  def respond_to? name
    puts "?S #{name}"
    super
  end

  def to_str
    puts '!S to_str'
    @val
  end
end

class A
  def initialize val
    @val = val
  end
  
  def respond_to? name
    puts "?A #{name}"
    super
  end

  def to_ary
    puts '!A to_ary'
    @val
  end
end

class AI
  def initialize array, int
    @array = array
    @int = int
  end
  
  def respond_to? name
    puts "?A #{name}"
    super
  end

  def to_ary
    puts '!A to_ary'
    @array
  end
  
  def to_int
    puts '!A to_int'
    @int
  end
end


class ES
  def initialize val
    @val = val
  end
  
  def respond_to? name
    puts "?ES #{name}"
    super
  end

  def to_s
    puts '!ES to_s'
    @val
  end
end

class W
  def write x
    puts ">#{x}<"
  end
end

class MyStr < String

end
