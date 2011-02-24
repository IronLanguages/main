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

class Sym
  def initialize sym, str = sym.to_s
    @str = str
    @sym = sym
  end
  
  def respond_to? name
    puts "?Sym #{name}"
    super
  end

  def to_str
    puts '!Sym to_str'
    @str
  end
  
  def to_sym
    puts '!Sym to_sym'
    @sym
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
