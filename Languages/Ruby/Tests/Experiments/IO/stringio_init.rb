require 'stringio'

class C
  def respond_to? name
    puts "?" + name.to_s
    false
  end
end

class D
  def to_str
    "r"
  end
end

p StringIO.new()
StringIO.new(C.new) rescue p $!
StringIO.new(nil) rescue p $!
StringIO.new("str", C.new) rescue p $!
p StringIO.new("str", D.new)
StringIO.new("str", nil) rescue p $!
p StringIO.new("str", 1)
StringIO.new("str", 1, 1) rescue p $!
StringIO.new(1) rescue p $!


